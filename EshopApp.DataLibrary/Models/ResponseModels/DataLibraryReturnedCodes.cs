namespace EshopApp.DataLibrary.Models.ResponseModels;

public enum DataLibraryReturnedCodes
{
    NoError,
    NoErrorButNotFullyDeleted,
    TheIdOfTheEntityCanNotBeNull,
    TheIdOfTheCouponCanNotBeNull,
    TheIdOfTheUserCanNotBeNull,
    EntityNotFoundWithGivenId,
    InvalidProductIdWasGiven,
    InvalidVariantIdWasGiven,
    InvalidCouponIdWasGiven,
    StartAndExpirationDatesCanNotBeNullForUniversalCoupons,
    DefaultDateIntervalInDaysCanNotBeNullForUserSpecificCoupons,
    NoVariantWasProvidedForProductCreation,
    DuplicateVariantSku,
    DuplicateEntityCode,
    DuplicateEntityName
}
