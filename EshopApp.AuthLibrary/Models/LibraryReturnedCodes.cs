namespace EshopApp.AuthLibrary.Models;

public enum LibraryReturnedCodes
{
    DuplicateEmail,
    UserNotFoundWithGivenId,
    UserNotFoundWithGivenEmail,
    UserAccountLocked,
    UserAccountNotActivated,
    InvalidCredentials,
    InvalidEmailAndEmailChangeTokenCombination,
    PasswordMissmatch,
    UserDoesNotOwnGivenAccount,
    LoginInfoNotReceivedFromIdentityProvider,
    EmailClaimNotReceivedFromIdentityProvider,
    ValidTokenButUserNotInSystem,
    UnknownError,
    NoError
}
