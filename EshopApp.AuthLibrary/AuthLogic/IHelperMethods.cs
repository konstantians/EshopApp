using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.AuthLogic;

public interface IHelperMethods
{
    Task<bool> IsAccountLockedOut(AppUser appUser, EventId eventId, string loggingBodyText);
    bool IsEmailConfirmed(AppUser user, EventId eventId, string loggingText);
    Task<ReturnUserAndCodeResponseModel> StandardTokenAndUserValidationProcedures(string accessToken, EventId templateEvent);
    Task<ReturnUserAndCodeResponseModel> StandardTokenValidationAuthenticationAndAuthorizationProcedures(string accessToken, List<Claim> expectedClaims, EventId templateEvent);
    Task<LibraryReturnedCodes> CheckIfAuthorizedToEditSpecificRole(AppUser editorUser, AppRole editedRole);
    Task<LibraryReturnedCodes> CheckIfAuthorizedToEditSpecificUser(AppUser editorUser, AppUser editedUser);
}