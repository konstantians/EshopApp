namespace EshopApp.DataLibrary.Models.ResponseModels;

public enum DataLibraryReturnedCodes
{
    NoError,
    TheIdOfTheEntityCanNotBeNull,
    EntityNotFoundWithGivenId,
    InvalidProductIdWasGiven,
    InvalidVariantIdWasGiven,
    NoVariantWasProvidedForProductCreation,
    DuplicateVariantSku,
    DuplicateProductCode,
    DuplicateEntityName
}
