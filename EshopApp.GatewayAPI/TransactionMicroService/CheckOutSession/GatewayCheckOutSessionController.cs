using EshopApp.GatewayAPI.AuthMicroService.Models;
using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.HelperMethods;
using EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession.Models.RequestModels;
using EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession.Models.ServiceRequestModels;
using EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession.Models.ServiceResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayCheckOutSessionController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly HttpClient transactionHttpClient;
    private readonly HttpClient emailHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayCheckOutSessionController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
        transactionHttpClient = httpClientFactory.CreateClient("TransactionApiClient");
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
    }

    [HttpPost]
    public async Task<IActionResult> CreateCheckOutSession(GatewayCreateCheckoutSessionRequestModel gatewayCreateCheckoutSessionRequestModel)
    {
        try
        {
            //Check that the success url and the cancel url are trusted
            if (!_utilityMethods.CheckIfUrlIsTrusted(gatewayCreateCheckoutSessionRequestModel.SuccessUrl!, _configuration))
                return BadRequest(new { ErrorMessage = "OriginForSuccessUrlIsNotTrusted" });
            if (!_utilityMethods.CheckIfUrlIsTrusted(gatewayCreateCheckoutSessionRequestModel.CancelUrl!, _configuration))
                return BadRequest(new { ErrorMessage = "OriginForCancelUrlIsNotTrusted" });

            //the usercouponcode field is the one that dictates what happens to the coupon and not the UserCouponId
            if (string.IsNullOrEmpty(gatewayCreateCheckoutSessionRequestModel.UserCouponCode) && gatewayCreateCheckoutSessionRequestModel.GatewayCreateOrderRequestModel!.UserCouponId is not null)
                return BadRequest(new { ErrorMessage = "IfTheUserCouponIdFieldIsNotNullThenTheUserCouponCodeShouldAlsoNotBeNull" });

            //start by doing healthchecks for the endpoints this is calling
            if (gatewayCreateCheckoutSessionRequestModel.UserCouponCode is not null && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, transactionHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            else if (gatewayCreateCheckoutSessionRequestModel.UserCouponCode is null && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { dataHttpClient, transactionHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            int couponPercentageDiscount = 0;
            //If Coupon is provided check that it is valid and it belongs to the user
            //also fill the couponPercentageDiscount
            if (!string.IsNullOrEmpty(gatewayCreateCheckoutSessionRequestModel.UserCouponCode))
            {
                //check that an access token has been supplied, this check is made to avoid unnecessary requests
                if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                    !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvidedWhileCouponCodeWasProvided" });

                //request to get the user
                _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
                HttpResponseMessage authenticationResponse = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Authentication/GetUserByAccessToken?includeCoupons=true")); //this contains retry logic

                if ((int)authenticationResponse.StatusCode >= 400)
                    return await _utilityMethods.CommonHandlingForErrorCodesAsync(authenticationResponse);

                string? authenticationResponseBody = await authenticationResponse.Content.ReadAsStringAsync();
                GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(authenticationResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                GatewayUserCoupon? gatewayUserCoupon = appUser!.UserCoupons.FirstOrDefault(userCoupon => userCoupon.Code == gatewayCreateCheckoutSessionRequestModel.UserCouponCode);
                if (gatewayUserCoupon is null)
                    return StatusCode(403, new { ErrorMessage = "UserAccountDoesNotContainThisCouponCode" });

                couponPercentageDiscount = gatewayUserCoupon.Coupon!.DiscountPercentage!.Value;
                gatewayCreateCheckoutSessionRequestModel.GatewayCreateOrderRequestModel!.UserCouponId = gatewayUserCoupon.Id;
            }

            //Create the order
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order", gatewayCreateCheckoutSessionRequestModel.GatewayCreateOrderRequestModel));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayOrder? order = JsonSerializer.Deserialize<GatewayOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //Create the session
            GatewayCreateCheckoutSessionServiceRequestModel gatewayCreateCheckoutSessionServiceRequestModel = new GatewayCreateCheckoutSessionServiceRequestModel();
            gatewayCreateCheckoutSessionServiceRequestModel.PaymentMethodType = gatewayCreateCheckoutSessionRequestModel.PaymentMethodType;
            gatewayCreateCheckoutSessionServiceRequestModel.SuccessUrl = gatewayCreateCheckoutSessionRequestModel.SuccessUrl;
            gatewayCreateCheckoutSessionServiceRequestModel.CancelUrl = gatewayCreateCheckoutSessionRequestModel.CancelUrl;
            gatewayCreateCheckoutSessionServiceRequestModel.CustomerEmail = gatewayCreateCheckoutSessionRequestModel.GatewayCreateOrderRequestModel!.Email;
            gatewayCreateCheckoutSessionServiceRequestModel.CouponPercentage = couponPercentageDiscount;

            gatewayCreateCheckoutSessionServiceRequestModel.PaymentOptionName = order!.PaymentDetails!.PaymentOption!.Name;
            gatewayCreateCheckoutSessionServiceRequestModel.PaymentOptionDescription = order.PaymentDetails!.PaymentOption!.Description;
            gatewayCreateCheckoutSessionServiceRequestModel.PaymentOptionCostInEuro = order.PaymentDetails!.PaymentOption!.ExtraCost;

            gatewayCreateCheckoutSessionServiceRequestModel.ShippingOptionName = order.ShippingOption!.Name;
            gatewayCreateCheckoutSessionServiceRequestModel.ShippingOptionDescription = order.ShippingOption.Description;
            gatewayCreateCheckoutSessionServiceRequestModel.ShippingOptionCostInEuro = order.ShippingOption.ExtraCost;

            foreach (GatewayOrderItem orderItem in order.OrderItems)
            {
                var gatewayCreateTransactionOrderItemServiceRequestModel = new GatewayCreateTransactionOrderItemServiceRequestModel();
                gatewayCreateTransactionOrderItemServiceRequestModel.Name = orderItem.Variant!.Product!.Name;
                gatewayCreateTransactionOrderItemServiceRequestModel.Description = orderItem.Variant!.Product!.Description;
                gatewayCreateTransactionOrderItemServiceRequestModel.Quantity = orderItem.Quantity;
                gatewayCreateTransactionOrderItemServiceRequestModel.FinalUnitAmountInEuro = orderItem.UnitPriceAtOrder;
                if (orderItem.Variant.Discount is not null)
                    gatewayCreateTransactionOrderItemServiceRequestModel.FinalUnitAmountInEuro -= orderItem.Variant.Price * orderItem.Variant.Discount.Percentage;

                gatewayCreateTransactionOrderItemServiceRequestModel.ImageUrl = orderItem.ImageId;

                gatewayCreateCheckoutSessionServiceRequestModel.CreateTransactionOrderItemRequestModels.Add(gatewayCreateTransactionOrderItemServiceRequestModel);
            }

            //Create the checkout session
            _utilityMethods.SetDefaultHeadersForClient(false, transactionHttpClient, _configuration["TransactionApiKey"]!, _configuration["TransactionRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => transactionHttpClient.PostAsJsonAsync("CheckOutSession", gatewayCreateCheckoutSessionServiceRequestModel));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            var gatewayCreateCheckoutSessionServiceResponseModel = JsonSerializer.Deserialize<GatewayCreateCheckoutSessionServiceResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //Attach the session id to the order
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order", new { Id = order.Id, PaymentProcessorSessionId = gatewayCreateCheckoutSessionServiceResponseModel!.CheckOutSessionId }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //maybe return redirect, but I will leave it us ok now
            return Ok(gatewayCreateCheckoutSessionServiceResponseModel!.CheckOutSessionUrl); //return to the client the CheckOutSessionUrl, so that they can redirect there
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    //this happens after the checkout session has been completed and the webhook has successfully being triggered and then trigger this endpoint
    //maybe do some authentication?
    [HttpPost("HandleCreateCheckOutSession")]
    public async Task<IActionResult> HandleCreateCheckOutSession(GatewayHandleCreateCheckOutSessionRequestModel gatewayHandleCreateCheckOutSessionRequestModel)
    {
        try
        {
            //start by doing healthchecks for the endpoints this is calling
            if (gatewayHandleCreateCheckOutSessionRequestModel.ShouldSendEmail && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { dataHttpClient, emailHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            else if (!gatewayHandleCreateCheckOutSessionRequestModel.ShouldSendEmail && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { dataHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //Update the Order
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order", new
            {
                PaymentProcessorSessionId = gatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorSessionId,
                PaymentProcessorPaymentIntentId = gatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorPaymentIntentId,
                PaymentStatus = gatewayHandleCreateCheckOutSessionRequestModel.NewPaymentStatus,
                PaymentCurrency = gatewayHandleCreateCheckOutSessionRequestModel.PaymentCurrency,
                AmountPaidInEuro = gatewayHandleCreateCheckOutSessionRequestModel.AmountPaidInEuro,
                NetAmountPaidInEuro = gatewayHandleCreateCheckOutSessionRequestModel.NetAmountPaidInEuro
            }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //Update the OrderStatus
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order/UpdateOrderStatus", new
            {
                PaymentProcessorSessionId = gatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorSessionId,
                PaymentStatus = gatewayHandleCreateCheckOutSessionRequestModel.NewPaymentStatus
            }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //Send Order Confirmation Email
            //send an email to the user to notify them that their order has been successfully placed(include order information)
            if (gatewayHandleCreateCheckOutSessionRequestModel.ShouldSendEmail)
            {
                //Get the order
                response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                    dataHttpClient.GetAsync($"Order/PaymentProcessorSessionId/{gatewayHandleCreateCheckOutSessionRequestModel.PaymentProcessorPaymentIntentId}"));

                string? responseBody = await response.Content.ReadAsStringAsync();
                GatewayOrder? order = JsonSerializer.Deserialize<GatewayOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if ((int)response.StatusCode >= 400)
                    return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

                var apiSendEmailModel = new Dictionary<string, string>
                {
                    { "receiver", order!.OrderAddress!.Email! },
                    { "title", "Order Placed Successfully" },
                    { "message", "Here are the details of your order: " } //TODO do this well
                };
                _ = Task.Run(async () =>
                {
                    _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                    await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
                });
            }

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
