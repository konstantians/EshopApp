using EshopApp.TransactionLibrary.Models;
using EshopApp.TransactionLibrary.Models.ResponseModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Stripe;
using Stripe.Checkout;

namespace EshopApp.TransactionLibrary.Services;
public class CheckOutSessionService : ICheckOutSessionService
{
    private readonly ILogger<CheckOutSessionService> _logger;

    public CheckOutSessionService(ILogger<CheckOutSessionService> logger = null!)
    {
        _logger = logger ?? NullLogger<CheckOutSessionService>.Instance;
    }

    //probably create the session before the local order is created, because the local order validates pretty much everything here(the gateway api can be trusted to send make the call to the transaction api correctly after testing)
    public async Task<ReturnSessionIdSessionUrlAndCodeResponseModel?> CreateCheckOutSessionAsync(CheckOutSession checkOutSession)
    {
        try
        {
            if (checkOutSession.TransactionOrderItems is null || !checkOutSession.TransactionOrderItems.Any())
                return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.ThereNeedsToBeAtLeastOneOrderItem);
            else if (checkOutSession.CustomerEmail is null)
                return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.NoCustomerEmailWasProvided);

            List<SessionLineItemOptions> sessionLineItemOptions = new List<SessionLineItemOptions>();
            foreach (TransactionOrderItem transactionOrderItem in checkOutSession.TransactionOrderItems)
            {
                if (transactionOrderItem.FinalUnitAmountInEuro <= 0)
                    return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.InvalidOrderItemPrice);
                else if (transactionOrderItem.Name is null)
                    return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.OrderItemNameIsMissing);
                else if (transactionOrderItem.Quantity <= 0)
                    return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.InvalidOrderItemQuantity);

                sessionLineItemOptions.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = "EUR",
                        UnitAmountDecimal = transactionOrderItem.FinalUnitAmountInEuro * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = transactionOrderItem.Name,
                            Description = transactionOrderItem.Description ?? "",
                            Images = new List<string>() { transactionOrderItem.ImageUrl! }
                        }
                    },
                    Quantity = transactionOrderItem.Quantity,
                });
            }

            if (checkOutSession.TransactionPaymentOption is not null && checkOutSession.TransactionPaymentOption.CostInEuro != 0)
            {
                if (checkOutSession.TransactionPaymentOption.CostInEuro < 0)
                    return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.InvalidPaymentOptionPrice);
                else if (checkOutSession.TransactionPaymentOption.Name is null)
                    return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.PaymentOptionNameIsMissing);

                sessionLineItemOptions.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = "EUR",
                        UnitAmountDecimal = checkOutSession.TransactionPaymentOption.CostInEuro * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = checkOutSession.TransactionPaymentOption.Name,
                            Description = checkOutSession.TransactionPaymentOption.Description ?? ""
                        }
                    },
                    Quantity = 1,
                });
            }

            if (checkOutSession.TransactionShippingOption is not null && checkOutSession.TransactionShippingOption.CostInEuro != 0)
            {
                if (checkOutSession.TransactionShippingOption.CostInEuro < 0)
                    return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.InvalidShippingOptionPrice);
                else if (checkOutSession.TransactionShippingOption.Name is null)
                    return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.ShippingOptionNameIsMissing);

                sessionLineItemOptions.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = "EUR",
                        UnitAmountDecimal = checkOutSession.TransactionShippingOption.CostInEuro * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = checkOutSession.TransactionShippingOption.Name,
                            Description = checkOutSession.TransactionShippingOption.Description ?? ""
                        }
                    },
                    Quantity = 1,
                });
            }

            if (checkOutSession.CouponPercentage is not null && checkOutSession.CouponPercentage.Value <= 0)
                return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.InvalidCouponDiscountPercentage);

            CouponCreateOptions? couponCreateOptions = checkOutSession is not null ? new CouponCreateOptions { PercentOff = checkOutSession.CouponPercentage, Duration = "once" } : null;
            List<SessionDiscountOptions> sessionDiscountOptions = new List<SessionDiscountOptions>();
            if (couponCreateOptions is not null)
            {
                CouponService couponService = new CouponService();
                Coupon coupon = await couponService.CreateAsync(couponCreateOptions);
                sessionDiscountOptions.Add(new SessionDiscountOptions() { Coupon = coupon.Id });
            }

            SessionCreateOptions options = new SessionCreateOptions
            {
                ExpiresAt = checkOutSession!.ExpiresAt,
                Discounts = sessionDiscountOptions,
                CustomerEmail = checkOutSession.CustomerEmail,
                SuccessUrl = checkOutSession.SuccessUrl,
                CancelUrl = checkOutSession.CancelUrl,
                PaymentMethodTypes = new List<string>() { checkOutSession.PaymentMethodType ?? "card" },
                LineItems = sessionLineItemOptions,
                Mode = "payment" //this means that the payment will always happen all at once through the session
            };

            SessionService sessionService = new SessionService();
            Session session = await sessionService.CreateAsync(options);

            _logger.LogInformation(new EventId(9999, "CreateCheckOutSessionSuccess"), "The check-out session was successfully created.");
            return new ReturnSessionIdSessionUrlAndCodeResponseModel(session.Id, session.Url, TransactionLibraryReturnedCodes.NoError);
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(new EventId(9999, "CreateCheckOutSessionFailureDueToStripeError"), "Stripe API error: {Message}", stripeEx.Message);
            return new ReturnSessionIdSessionUrlAndCodeResponseModel(null!, null!, TransactionLibraryReturnedCodes.StripeApiError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateCheckOutSessionFailure"), ex, "An error occurred while creating the check-out session. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }
}
