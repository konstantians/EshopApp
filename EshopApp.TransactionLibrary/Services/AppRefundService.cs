using EshopApp.TransactionLibrary.Models.ResponseModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stripe;
using Stripe.Checkout;

namespace EshopApp.TransactionLibrary.Services;
public class AppRefundService : IAppRefundService
{
    private readonly ILogger<AppRefundService> _logger;
    public AppRefundService(ILogger<AppRefundService> logger = null!)
    {
        _logger = logger ?? NullLogger<AppRefundService>.Instance;
    }

    public async Task<TransactionLibraryReturnedCodes> IssueRefundAsync(string sessionId)
    {
        try
        {
            SessionService sessionService = new SessionService();
            Session? session = await sessionService.GetAsync(sessionId);
            if (session is null)
            {
                _logger.LogWarning(new EventId(9999, "IssueRefundFailureDueToNullCheckOutSession"), "There is no corresponding checkoutsession in Stripe with Id={sessionId} and thus the refund process could not proceed", sessionId);
                return TransactionLibraryReturnedCodes.CheckOutSessionNotFoundWithGivenId;
            }

            string paymentIntentId = session.PaymentIntentId;
            var amountPaidByUser = session.PaymentIntent.AmountReceived;

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = amountPaidByUser
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
