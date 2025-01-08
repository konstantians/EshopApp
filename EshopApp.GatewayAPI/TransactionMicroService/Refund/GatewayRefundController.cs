using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.HelperMethods;
using EshopApp.GatewayAPI.TransactionMicroService.Refund.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;

namespace EshopApp.GatewayAPI.TransactionMicroService.Discount;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayRefundController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly HttpClient transactionHttpClient;
    private readonly HttpClient emailHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayRefundController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
        transactionHttpClient = httpClientFactory.CreateClient("TransactionApiClient");
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
    }

    [HttpPost]
    public async Task<IActionResult> IssueRefund(GatewayIssueRefundRequestModel gatewayIssueRefundRequestModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //start by doing healthchecks for the endpoints this is calling
            if (!await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, transactionHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //here there is no reason to check if both microservices are fully online since changes can only occur in the second and final call
            //authenticate and authorize the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.GetAsync("Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/claimType/Permission/claimValue/CanManageOrders"));

            //Create the refund
            _utilityMethods.SetDefaultHeadersForClient(false, transactionHttpClient, _configuration["TransactionApiKey"]!, _configuration["TransactionRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => transactionHttpClient.PostAsJsonAsync("Refund", gatewayIssueRefundRequestModel));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //TODO maybe do a check if the webhook manages to do its work faster than this?
            //Update the order status of the order to RefundPending
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order/UpdateOrderStatus", new
            {
                OrderId = gatewayIssueRefundRequestModel.OrderId,
                PaymentStatus = "RefundPending"
            }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    //this happens after the refund process has completed(from the endpoint) and the webhook has successfully being triggered and then trigger this endpoint
    //maybe do some authentication?
    [HttpPost("HandleIssueRefund")]
    public async Task<IActionResult> HandleIssueRefund(GatewayHandleIssureRefundRequestModel gatewayHandleIssureRefundRequestModel)
    {
        try
        {
            //start by doing healthchecks for the endpoints this is calling
            if (gatewayHandleIssureRefundRequestModel.ShouldSendEmail && await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { dataHttpClient, emailHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });
            else if (!gatewayHandleIssureRefundRequestModel.ShouldSendEmail && !(await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { dataHttpClient })))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //Update the order status accordingly(there are 2 options either succeeded or failed). This update will also update the ammounts accordingly
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PutAsJsonAsync("Order/UpdateOrderStatus", new
            {
                PaymentProcessorSessionId = gatewayHandleIssureRefundRequestModel.PaymentIntentId,
                NewOrderStatus = gatewayHandleIssureRefundRequestModel.NewOrderStatus
            }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //Send Refund Confirmation Email If Order Successfully redunded
            //send an email to the user to notify them that their order has been refunded
            if (gatewayHandleIssureRefundRequestModel.ShouldSendEmail && gatewayHandleIssureRefundRequestModel.NewOrderStatus == "Refunded")
            {
                response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                    dataHttpClient.GetAsync($"Order/PaymentProcessorPaymentIntentId/{gatewayHandleIssureRefundRequestModel.PaymentIntentId}"));

                if ((int)response.StatusCode >= 400)
                    return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

                string? responseBody = await response.Content.ReadAsStringAsync();
                GatewayOrder? order = JsonSerializer.Deserialize<GatewayOrder>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var apiSendEmailModel = new Dictionary<string, string>
                {
                    { "receiver", order!.OrderAddress!.Email! },
                    { "title", "Order Refunded Successfully" },
                    { "message", "Your order has been successfully refunded. If you have any question please contact us on kinnaskonstantinos0@gmail.com" }
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
