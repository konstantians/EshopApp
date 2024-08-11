namespace EshopApp.AuthLibrary.Models.ResponseModels;

public enum LibraryReturnedCodes
{
    DuplicateEmail,
    DuplicateRole,
    UserNotFoundWithGivenId,
    UserNotFoundWithGivenEmail,
    RoleNotFoundWithGivenId,
    UserAccountLocked,
    UserAccountNotActivated,
    InvalidCredentials,
    InvalidEmailAndEmailChangeTokenCombination,
    PasswordMissmatch,
    UserDoesNotOwnGivenAccount,
    LoginInfoNotReceivedFromIdentityProvider,
    EmailClaimNotReceivedFromIdentityProvider,
    ValidTokenButUserNotInSystem,
    ValidTokenButClaimNotInSystem,
    UnknownError,
    NoError
}
