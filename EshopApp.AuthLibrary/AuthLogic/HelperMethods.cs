using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.AuthLogic;

public class HelperMethods : IHelperMethods
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ILogger<AuthenticationProcedures> _logger;

    public HelperMethods(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ILogger<AuthenticationProcedures> logger = null!)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
    }

    public bool IsEmailConfirmed(AppUser appUser, EventId eventId, string loggingBodyText)
    {
        if (!appUser.EmailConfirmed)
        {
            _logger.LogWarning(eventId, loggingBodyText, appUser.Email);
            return false;
        }

        return true;
    }

    public async Task<bool> IsAccountLockedOut(AppUser appUser, EventId eventId, string loggingBodyText)
    {
        if (await _userManager.IsLockedOutAsync(appUser))
        {
            _logger.LogWarning(eventId, loggingBodyText, appUser.Email);
            return true;
        }

        return false;
    }

    public async Task<ReturnUserAndCodeResponseModel> StandardTokenAndUserValidationProcedures(string accessToken, EventId templateEvent)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            _logger.LogWarning(new EventId(templateEvent.Id, templateEvent.Name + "FailureDueToValidTokenButUserNotInSystem"), 
                "The token was valid, but it does not correspond to any user in the system and thus the process could not proceed. AccessToken={AccessToken}.", accessToken);
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.ValidTokenButUserNotInSystem);
        }

        if (IsEmailConfirmed(appUser, new EventId(templateEvent.Id + 1, templateEvent.Name + "FailureDueToUnconfirmedEmail"), 
            "The email of the account was not confirmed and thus the process could not proceed. Email={Email}."))
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

        if(await IsAccountLockedOut(appUser, new EventId(templateEvent.Id + 2, templateEvent.Name + "FailureDueToAccountBeingLocked"), 
            "The account was locked out at the time of the process and thus the process could not proceed: Email={Email}."))
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

        return new ReturnUserAndCodeResponseModel(appUser, LibraryReturnedCodes.NoError);
    }

    public async Task<ReturnUserAndCodeResponseModel> StandardTokenValidationAuthenticationAndAuthorizationProcedures(string accessToken, List<Claim> expectedClaims, EventId templateEvent)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        string userId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value!;
        string roleName = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value!;
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            _logger.LogWarning(new EventId(templateEvent.Id, templateEvent.Name + "FailureDueToValidTokenButUserNotInSystem"),
                "The token was valid, but it does not correspond to any user in the system and thus the process could not proceed. AccessToken={AccessToken}.", accessToken);
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.ValidTokenButUserNotInSystem);
        }

        var userRole = await _roleManager.FindByNameAsync(roleName);

        bool result = await _userManager.IsInRoleAsync(appUser, roleName);
        if (!result) 
        {
            _logger.LogWarning(new EventId(templateEvent.Id + 2, templateEvent.Name + "FailureDueToRoleNotInUserRolesInSystem"),
                "The role existed correctly in the access token, but the user is not connected to the role with RoleName={RoleName} in the database. AccessToken={AccessToken}.", roleName, accessToken);
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.ValidTokenButUserNotInSystem);
        }

        var userRoleClaimsInSystem = await _roleManager.GetClaimsAsync(userRole!);
        foreach (var expectedClaim in expectedClaims) 
        { 
            if(!userRoleClaimsInSystem.Any(userRoleClaim => userRoleClaim.Type == expectedClaim.Type && userRoleClaim.Value == expectedClaim.Value))
            {
                _logger.LogWarning(new EventId(templateEvent.Id + 3, templateEvent.Name + "FailureDueToClaimNotPartOfRoleInSystem"),
                    "The claims existed correctly in the access token, but the claim with ClaimType={ClaimType} and ClaimValue={ClaimValue} is not currently part of the role with RoleName={RoleName}. " +
                    "AccessToken={AccessToken}.", expectedClaim.Type, expectedClaim.Value, roleName, accessToken);
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.ValidTokenButUserNotInSystem);
            }
        }

        if (IsEmailConfirmed(appUser, new EventId(templateEvent.Id + 4, templateEvent.Name + "FailureDueToUnconfirmedEmail"),
            "The email of the account was not confirmed and thus the process could not proceed. Email={Email}."))
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

        if (await IsAccountLockedOut(appUser, new EventId(templateEvent.Id + 5, templateEvent.Name + "FailureDueToAccountBeingLocked"),
            "The account was locked out at the time of the process and thus the process could not proceed: Email={Email}."))
            return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

        return new ReturnUserAndCodeResponseModel(appUser, LibraryReturnedCodes.NoError);
    }

}
