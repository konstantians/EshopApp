using EshopApp.TransactionLibrary.Models;
using EshopApp.TransactionLibrary.Models.ResponseModels;
using EshopApp.TransactionLibrary.Services;
using EshopApp.TransactionLibraryAPI.Models.RequestModels;
using EshopApp.TransactionLibraryAPI.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stripe;
using Stripe.Checkout;

namespace EshopApp.TransactionLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class CheckOutSessionController : ControllerBase
{
    private readonly ICheckOutSessionService _checkOutSessionService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient? httpClient;

    public CheckOutSessionController(ICheckOutSessionService checkOutSessionService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _checkOutSessionService = checkOutSessionService;
        _configuration = configuration;

        if (!string.IsNullOrEmpty(configuration["ApiClientBaseUrl"]))
            httpClient = httpClientFactory.CreateClient("ApiClientBaseUrl");
    }

    [HttpPost]
    public async Task<IActionResult> CreateCheckOutSession(CreateCheckOutSessionRequestModel createCheckOutSessionRequestModel)
    {
        try
        {
            CheckOutSession checkOutSession = new CheckOutSession();
            checkOutSession.PaymentMethodType = createCheckOutSessionRequestModel.PaymentMethodType;
            checkOutSession.SuccessUrl = createCheckOutSessionRequestModel.SuccessUrl;
            checkOutSession.CancelUrl = createCheckOutSessionRequestModel.CancelUrl;
            checkOutSession.CustomerEmail = createCheckOutSessionRequestModel.CustomerEmail;
            checkOutSession.ExpiresAt = createCheckOutSessionRequestModel.ExpiresAt ?? checkOutSession.ExpiresAt;
            checkOutSession.CouponPercentage = createCheckOutSessionRequestModel.CouponPercentage; //can be null
            checkOutSession.TransactionPaymentOption = new TransactionPaymentOption
            {
                Name = createCheckOutSessionRequestModel.PaymentOptionName,
                Description = createCheckOutSessionRequestModel.PaymentOptionDescription,
                CostInEuro = createCheckOutSessionRequestModel.PaymentOptionCostInEuro!.Value
            };
            checkOutSession.TransactionShippingOption = new TransactionShippingOption
            {
                Name = createCheckOutSessionRequestModel.ShippingOptionName,
                Description = createCheckOutSessionRequestModel.ShippingOptionDescription,
                CostInEuro = createCheckOutSessionRequestModel.ShippingOptionCostInEuro!.Value
            };

            foreach (var createTransactionOrderItemRequestModel in createCheckOutSessionRequestModel.CreateTransactionOrderItemRequestModels)
            {
                TransactionOrderItem transactionOrderItem = new TransactionOrderItem();
                transactionOrderItem.Name = createTransactionOrderItemRequestModel.Name;
                transactionOrderItem.Description = createTransactionOrderItemRequestModel.Description;
                transactionOrderItem.ImageUrl = createTransactionOrderItemRequestModel.ImageUrl;
                transactionOrderItem.Quantity = createTransactionOrderItemRequestModel.Quantity!.Value;
                transactionOrderItem.FinalUnitAmountInEuro = createTransactionOrderItemRequestModel.FinalUnitAmountInEuro!.Value;

                checkOutSession.TransactionOrderItems.Add(transactionOrderItem);
            }

            ReturnSessionIdSessionUrlAndCodeResponseModel? responseModel = await _checkOutSessionService.CreateCheckOutSessionAsync(checkOutSession);

            if (responseModel!.ReturnedCode == TransactionLibraryReturnedCodes.ThereNeedsToBeAtLeastOneOrderItem)
                return BadRequest(new { ErrorMessage = "ThereNeedsToBeAtLeastOneOrderItem" });
            else if (responseModel!.ReturnedCode == TransactionLibraryReturnedCodes.StripeApiError)
                return StatusCode(500, new { ErrorMessage = "StripeApiError" });

            return Created("", new CreateCheckOutSessionResponseModel()
            {
                CheckOutSessionId = responseModel!.CheckOutSessionId,
                CheckOutSessionUrl = responseModel!.CheckOutSessionUrl
            });
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleCheckOutSessionEvent()
    {
        try
        {
            string endpointSecret = _configuration["HandleCheckOutSessionEventSecret"]!; //each endpoint has its own secret key
            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                json = await reader.ReadToEndAsync(); //read the body of the event
            }

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret, throwOnApiVersionMismatch: false); //pass in the json body, the stripe signature and the endpoint secret
            bool.TryParse(_configuration["ShouldRedirectEndpointSentEmail"], out bool shouldSendEmail);

            if (stripeEvent.Type == "checkout.session.completed" && !string.IsNullOrEmpty(_configuration["ApiClientBaseUrl"]) && !string.IsNullOrEmpty(_configuration["SessionCompletedRedirectEndpoint"]))
            {
                Session? session = stripeEvent.Data.Object as Session;
                PaymentIntentService paymentIntentService = new PaymentIntentService();
                PaymentIntent paymentIntent = await paymentIntentService.GetAsync(session!.PaymentIntentId!, new PaymentIntentGetOptions()
                {
                    Expand = new List<string> { "latest_charge", "latest_charge.balance_transaction" }
                });

                int retries = 5;
                while ((paymentIntent.LatestCharge == null || paymentIntent.LatestCharge.BalanceTransaction == null) && retries > 0)
                {
                    await Task.Delay(1500);
                    paymentIntent = await paymentIntentService.GetAsync(session!.PaymentIntentId!, new PaymentIntentGetOptions()
                    {
                        Expand = new List<string> { "latest_charge", "latest_charge.balance_transaction" }
                    });
                    retries--;
                }

                long fee = paymentIntent.LatestCharge is not null && paymentIntent.LatestCharge.BalanceTransaction is not null ? paymentIntent.LatestCharge!.BalanceTransaction!.Fee : 0; //in that case of 0 I suppose the admin will need to check it

                var responseModel = new HandleCheckOutSessionResponseModel
                {
                    PaymentProcessorPaymentIntentId = paymentIntent.Id,
                    PaymentProcessorSessionId = session?.Id,
                    NewOrderStatus = "Confirmed",
                    NewPaymentStatus = session?.PaymentStatus,
                    PaymentCurrency = session?.Currency,
                    AmountPaidInEuro = paymentIntent.AmountReceived / 100.0m,
                    NetAmountPaidInEuro = (paymentIntent.AmountReceived - fee) / 100.0m,
                    ShouldSendEmail = shouldSendEmail
                };

                string fullUrl = _configuration["ApiClientBaseUrl"]!.EndsWith('/') ? _configuration["ApiClientBaseUrl"] + _configuration["SessionCompletedRedirectLink"] :
                    _configuration["ApiClientBaseUrl"] + "/" + _configuration["SessionCompletedRedirectLink"];
                httpClient!.DefaultRequestHeaders.Add("X-API-KEY", _configuration["ApiClientApiKey"]);
                httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", _configuration["ApiClientRateLimitingBypassCode"]);
                await httpClient!.PostAsJsonAsync(fullUrl, responseModel);
            }
            else if (stripeEvent.Type == "checkout.session.expired" && !string.IsNullOrEmpty(_configuration["SessionExpiredRedirectEndpoint"]))
            {
                Session? session = stripeEvent.Data.Object as Session;
                PaymentIntent? paymentIntent = session?.PaymentIntent;

                var responseModel = new HandleCheckOutSessionResponseModel
                {
                    PaymentProcessorSessionId = session?.Id,
                    NewOrderStatus = "Failed", //maybe add expired status eventually
                    NewPaymentStatus = session?.PaymentStatus,
                    ShouldSendEmail = shouldSendEmail
                };

                string fullUrl = _configuration["ApiClientBaseUrl"]!.EndsWith('/') ? _configuration["ApiClientBaseUrl"] + _configuration["SessionExpiredRedirectEndpoint"] :
                    _configuration["ApiClientBaseUrl"] + "/" + _configuration["SessionExpiredRedirectEndpoint"];
                httpClient!.DefaultRequestHeaders.Add("X-API-KEY", _configuration["ApiClientApiKey"]);
                httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", _configuration["ApiClientRateLimitingBypassCode"]);
                await httpClient!.PostAsJsonAsync(fullUrl, responseModel);
            }

            return NoContent();
        }
        catch
        {
            return StatusCode(500);
        }
    }
}
