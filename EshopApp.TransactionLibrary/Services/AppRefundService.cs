using EshopApp.TransactionLibrary.Models.ResponseModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stripe;
using System.Net;

namespace EshopApp.TransactionLibrary.Services;
public class AppRefundService : IAppRefundService
{
    private readonly ILogger<AppRefundService> _logger;
    public AppRefundService(ILogger<AppRefundService> logger = null!)
    {
        _logger = logger ?? NullLogger<AppRefundService>.Instance;
    }

    public async Task<TransactionLibraryReturnedCodes> IssueRefundAsync(string paymentIntentId)
    {
        try
        {
            PaymentIntentService paymentIntentService = new PaymentIntentService();
            PaymentIntent? paymentIntent = await paymentIntentService.GetAsync(paymentIntentId); //we check for null in the exception

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = paymentIntent.AmountReceived
            };

            RefundService refundService = new RefundService();
            Refund refund = await refundService.CreateAsync(options);
            if (refund is null || (refund.Status != "pending" && refund.Status != "succeeded"))
                return TransactionLibraryReturnedCodes.RefundInvalidStateAfterCreation;

            _logger.LogInformation(new EventId(9999, "IssueRefundSuccess"), "The refund was successfully issued.");
            return TransactionLibraryReturnedCodes.NoError;
        }
        catch (StripeException stripeEx)
        {
            if (stripeEx.HttpStatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning(new EventId(9999, "IssueRefundFailureDueToNullPaymentIntent"), "There is no corresponding payment intent in Stripe with Id={paymentIntentId} and thus the refund process could not proceed", paymentIntentId);
                return TransactionLibraryReturnedCodes.PaymentIntentNotFoundWithGivenId;
            }

            _logger.LogError(new EventId(9999, "IssueRefundFailureDueToStripeError"), "Stripe API error: {Message}", stripeEx.Message);
            return TransactionLibraryReturnedCodes.StripeApiError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "IssueRefundFailure"), ex, "An error occurred while issuing the refund. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }
}
