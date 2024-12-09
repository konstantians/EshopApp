namespace EshopApp.TransactionLibrary.Models.ResponseModels;
public enum TransactionLibraryReturnedCodes
{
    NoError,
    ThereNeedsToBeAtLeastOneOrderItem,
    NoCustomerEmailWasProvided,
    InvalidOrderItemPrice,
    OrderItemNameIsMissing,
    InvalidOrderItemQuantity,
    InvalidPaymentOptionPrice,
    PaymentOptionNameIsMissing,
    InvalidShippingOptionPrice,
    ShippingOptionNameIsMissing,
    InvalidCouponDiscountPercentage,
    StripeApiError,
    CheckOutSessionNotFoundWithGivenId,
    PaymentIntentNotFoundWithGivenId,
    CheckOutSessionHasExpired,
    CheckOutSessionAlreadyCompleted,
    RefundInvalidStateAfterCreation
}
