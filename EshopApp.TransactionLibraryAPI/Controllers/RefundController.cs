using EshopApp.TransactionLibrary.Models.ResponseModels;
using EshopApp.TransactionLibrary.Services;
using EshopApp.TransactionLibraryAPI.Models.RequestModels;
using EshopApp.TransactionLibraryAPI.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stripe;

namespace EshopApp.TransactionLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class RefundController : ControllerBase
{
    private readonly IAppRefundService _appRefundService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient? httpClient;

    public RefundController(IAppRefundService appRefundService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _appRefundService = appRefundService;
        _configuration = configuration;

        if (!string.IsNullOrEmpty(configuration["ApiClientBaseUrl"]))
            httpClient = httpClientFactory.CreateClient("ApiClientBaseUrl");
    }

    [HttpPost]
    public async Task<IActionResult> IssueRefund(IssueRefundRequestModel issueRefundRequestModel)
    {
        try
        {
            TransactionLibraryReturnedCodes? returnedCode = await _appRefundService.IssueRefundAsync(issueRefundRequestModel.PaymentIntentId!);

            if (returnedCode == TransactionLibraryReturnedCodes.PaymentIntentNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "PaymentIntentNotFoundWithGivenId" });
            else if (returnedCode == TransactionLibraryReturnedCodes.RefundInvalidStateAfterCreation)
                return StatusCode(500, new { ErrorMessage = "RefundInvalidStateAfterCreation" });
            else if (returnedCode == TransactionLibraryReturnedCodes.ThereNeedsToBeAtLeastOneOrderItem)
                return StatusCode(500, new { ErrorMessage = "StripeApiError" });

            return NoContent();
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleIssueRefundEvent()
    {
        try
        {
            string endpointSecret = _configuration["HandleIssueRefundEventSecret"]!; //each endpoint has its own secret key
            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                json = await reader.ReadToEndAsync(); //read the body of the event
            }

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret, throwOnApiVersionMismatch: false); //pass in the json body, the stripe signature and the endpoint secret
            bool.TryParse(_configuration["ShouldRedirectEndpointSentEmail"], out bool shouldSendEmail);

            if (stripeEvent.Type == "refund.created" || stripeEvent.Type == "refund.updated" && !string.IsNullOrEmpty(_configuration["ApiClientBaseUrl"]) && !string.IsNullOrEmpty(_configuration["RefundSucceededRedirectEndpoint"]))
            {
                Refund? refund = stripeEvent.Data.Object as Refund;
                if (refund!.Status != "succeeded")
                    return NoContent();

                var responseModel = new HandleIssueRefundEventResponseModel
                {
                    NewOrderState = "Refunded",
                    PaymentIntentId = refund?.PaymentIntentId,
                    ShouldSendEmail = shouldSendEmail,
                };

                string fullUrl = _configuration["ApiClientBaseUrl"]!.EndsWith('/') ? _configuration["ApiClientBaseUrl"] + _configuration["RefundSucceededRedirectEndpoint"] :
                    _configuration["ApiClientBaseUrl"] + "/" + _configuration["RefundSucceededRedirectEndpoint"];
                httpClient!.DefaultRequestHeaders.Add("X-API-KEY", _configuration["ApiClientApiKey"]);
                httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", _configuration["ApiClientRateLimitingBypassCode"]);
                await httpClient!.PostAsJsonAsync(fullUrl, responseModel);
            }
            else if (stripeEvent.Type == "refund.failed" && !string.IsNullOrEmpty(_configuration["RefundFailedsRedirectEndpoint"]))
            {
                Refund? refund = stripeEvent.Data.Object as Refund;

                var responseModel = new HandleIssueRefundEventResponseModel
                {
                    NewOrderState = "RefundFailed",
                    PaymentIntentId = refund?.PaymentIntentId,
                    ShouldSendEmail = shouldSendEmail,
                };

                string fullUrl = _configuration["ApiClientBaseUrl"]!.EndsWith('/') ? _configuration["ApiClientBaseUrl"] + _configuration["RefundFailedsRedirectEndpoint"] :
                    _configuration["ApiClientBaseUrl"] + "/" + _configuration["RefundFailedsRedirectEndpoint"];
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
