namespace EshopApp.DataLibrary.Models.ResponseModels;

public enum DataLibraryReturnedCodes
{
    NoError,
    NoErrorButNotFullyDeleted,
    TheIdOfTheEntityCanNotBeNull,
    EntityNotFoundWithGivenId,
    InvalidProductIdWasGiven,
    InvalidVariantIdWasGiven,
    NoVariantWasProvidedForProductCreation,
    DuplicateVariantSku,
    DuplicateProductCode,
    DuplicateEntityName
}
