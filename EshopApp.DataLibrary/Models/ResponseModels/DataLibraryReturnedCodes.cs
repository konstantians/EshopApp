﻿namespace EshopApp.DataLibrary.Models.ResponseModels;

public enum DataLibraryReturnedCodes
{
    NoError,
    NoErrorButNotFullyDeleted,
    TheIdOfTheEntityCanNotBeNull,
    TheIdOfTheCouponCanNotBeNull,
    TheIdOfTheUserCanNotBeNull,
    EntityNotFoundWithGivenId,
    OrderNotFoundWithGivenPaymentProcessorSessionId,
    InvalidProductIdWasGiven,
    InvalidVariantIdWasGiven,
    InvalidCouponIdWasGiven,
    StartAndExpirationDatesCanNotBeNullForUniversalCoupons,
    DefaultDateIntervalInDaysCanNotBeNullForUserSpecificCoupons,
    NoVariantWasProvidedForProductCreation,
    DuplicateVariantSku,
    DuplicateEntityCode,
    DuplicateEntityName,
    DuplicateEntityNameAlias,
    ThePaymentDetailsObjectCanNotBeNull,
    TheIdOfThePaymentOptionCanNotBeNull,
    TheOrderAddressObjectCanNotBeNull,
    TheIdOfTheShippingOptionCanNotBeNull,
    TheOrderMustHaveAtLeastOneOrderItem,
    TheOrderItemsOfTheOrderWereAllInvalid,
    InvalidPaymentOption,
    InvalidShippingOption,
    CouponCodeCurrentlyDeactivated,
    CouponUsageLimitExceeded,
    OrderStatusHasBeenFinalizedAndThusTheOrderCanNotBeAltered,
    AllTheAltFieldsNeedToBeFilledIfDifferentShippingAddress,
    InvalidOrderStatus,
    OrderStatusHasBeenFinalizedAndThusOrderStatusCanNotBeAltered,
    ThisOrderDoesNotContainShippingAndThusTheShippedStatusIsInvalid,
    InvalidNewOrderState,
    TheOrderIdAndThePaymentProcessorSessionIdCanNotBeBothNull,
    InsufficientStockForVariant,
    InvalidCartIdWasGiven,
    UserAlreadyHasACart
}
