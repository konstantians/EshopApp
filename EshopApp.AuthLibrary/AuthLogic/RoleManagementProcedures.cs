using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibrary.Models.ResponseModels.RoleManagementModels;
using EshopApp.AuthLibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.AuthLogic;

//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 400-499, for example 2400 or 2499.
public class RoleManagementProcedures : IRoleManagementProcedures
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppIdentityDbContext _appIdentityDbContext;
    private readonly ILogger<AuthenticationProcedures> _logger;
    private readonly IHelperMethods _helperMethods;

    public RoleManagementProcedures(RoleManager<AppRole> identityUserRole, UserManager<AppUser> userManager,
        AppIdentityDbContext appIdentityDbContext, IHelperMethods helperMethods, ILogger<AuthenticationProcedures> logger = null!)
    {
        _roleManager = identityUserRole;
        _userManager = userManager;
        _appIdentityDbContext = appIdentityDbContext;
        _helperMethods = helperMethods;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
    }

    public async Task<ReturnRolesAndCodeResponseModel> GetRolesAsync(string accessToken, List<Claim> expectedClaims)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3400, "GetRoles"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnRolesAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            List<AppRole> appRoles = await _roleManager.Roles.ToListAsync();

            foreach (AppRole appRole in appRoles ?? Enumerable.Empty<AppRole>()) //this is a fancy way of checking if there are no roles(appRoles would be null in that case)
            {
                var returnedRoleClaims = await _roleManager.GetClaimsAsync(appRole);

                if (returnedRoleClaims is null)
                    continue;

                foreach (Claim returnedRoleClaim in returnedRoleClaims)
                    appRole.Claims.Add(new CustomClaim(returnedRoleClaim));

                var usersInRoleCount = await _userManager.GetUsersInRoleAsync(appRole.Name!);
                appRole.UsersInRoleCount = usersInRoleCount.Count;
            }

            //get the claims of the user that is trying to read the roles
            IList<string> editorUserRoleNames = await _userManager.GetRolesAsync(returnCodeAndUserResponseModel.AppUser!);
            AppRole? editorUserRole = editorUserRoleNames is null || editorUserRoleNames.Count == 0 ? null : await _roleManager.FindByNameAsync(editorUserRoleNames.FirstOrDefault()!);
            IList<Claim>? editorUserClaims = editorUserRole is null ? null : await _roleManager.GetClaimsAsync(editorUserRole);

            if (appRoles is not null && (editorUserClaims is null || !editorUserClaims.Any(editorClaim => editorClaim.Type == "Permission" && editorClaim.Value == "CanManageElevatedRoles")))
                appRoles.RemoveAll(role => role.Claims.Any(claim => claim.Value!.Contains("Elevated")));

            return new ReturnRolesAndCodeResponseModel(appRoles!, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4400, "GetRolesFailure"), ex, "An error occurred while retrieving the roles. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnRoleAndCodeResponseModel> GetRoleByIdAsync(string accessToken, List<Claim> expectedClaims, string roleId)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3405, "GetRoleById"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnRoleAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            AppRole? foundRole = await _roleManager.FindByIdAsync(roleId);

            if (foundRole is null)
                return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.NoError);

            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundRole);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return new ReturnRoleAndCodeResponseModel(null!, returnedCode);

            var usersInRoleCount = await _userManager.GetUsersInRoleAsync(foundRole.Name!);
            foundRole.UsersInRoleCount = usersInRoleCount.Count;

            var returnedRoleClaims = await _roleManager.GetClaimsAsync(foundRole);

            foreach (Claim returnedRoleClaim in returnedRoleClaims ?? Enumerable.Empty<Claim>())
                foundRole.Claims.Add(new CustomClaim(returnedRoleClaim));

            return new ReturnRoleAndCodeResponseModel(foundRole, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4401, "GetRoleByIdFailure"), ex, "An error occurred while retrieving role with RoleId={RoleId}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", roleId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnRoleAndCodeResponseModel> GetRoleByNameAsync(string accessToken, List<Claim> expectedClaims, string roleName)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3410, "GetRoleByName"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnRoleAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            AppRole? foundRole = await _roleManager.FindByNameAsync(roleName);
            if (foundRole is null)
                return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.NoError);


            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundRole);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return new ReturnRoleAndCodeResponseModel(null!, returnedCode);

            var usersInRoleCount = await _userManager.GetUsersInRoleAsync(foundRole.Name!);
            foundRole.UsersInRoleCount = usersInRoleCount.Count;

            var returnedRoleClaims = await _roleManager.GetClaimsAsync(foundRole);

            foreach (Claim returnedRoleClaim in returnedRoleClaims ?? Enumerable.Empty<Claim>())
                foundRole.Claims.Add(new CustomClaim(returnedRoleClaim));

            return new ReturnRoleAndCodeResponseModel(foundRole, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4402, "GetRoleByIdFailure"), ex, "An error occurred while retrieving role with the RoleName={RoleName}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", roleName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnRolesAndCodeResponseModel> GetRolesOfUserAsync(string accessToken, List<Claim> expectedClaims, string userId)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3415, "GetRolesOfUser"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnRolesAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            AppUser? foundUser = await _userManager.FindByIdAsync(userId);
            if (foundUser is null)
            {
                _logger.LogWarning(new EventId(3420, "GetRolesOfUserFailureDueToNullUser"), "Tried to retrieve roles of null user. UserId={UserId}", userId);
                return new ReturnRolesAndCodeResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
            }

            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificUser(returnCodeAndUserResponseModel.AppUser!, foundUser);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return new ReturnRolesAndCodeResponseModel(null!, returnedCode);

            List<string> userAppRolesNames = new List<string>(await _userManager.GetRolesAsync(foundUser));
            if (userAppRolesNames.Count == 0)
                return new ReturnRolesAndCodeResponseModel(null!, LibraryReturnedCodes.NoError);

            List<AppRole?> appRoles = new List<AppRole?>();
            foreach (var userAppRoleName in userAppRolesNames)
            {
                var appRole = await _roleManager.FindByNameAsync(userAppRoleName);
                List<Claim> roleClaims = new List<Claim>(await _roleManager.GetClaimsAsync(appRole!));

                if (roleClaims is null)
                    continue;

                foreach (Claim returnedRoleClaim in roleClaims)
                    appRole!.Claims.Add(new CustomClaim(returnedRoleClaim));

                appRoles.Add(appRole);
            }

            return new ReturnRolesAndCodeResponseModel(appRoles!, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4403, "GetRolesByIdFailure"), ex, "An error occurred while retrieving the role of user with UserId={UserId}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnRoleAndCodeResponseModel> CreateRoleAsync(string accessToken, List<Claim> expectedClaims, string roleName, List<CustomClaim> roleClaims)
    {
        var executionStrategy = _appIdentityDbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _appIdentityDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
                    ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3421, "CreateRole"));
                    if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                        return new ReturnRoleAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

                    //find the priviledges of the user(if the are elevated or not
                    IList<string> editorUserRoleNames = await _userManager.GetRolesAsync(returnCodeAndUserResponseModel.AppUser!);
                    AppRole? editorUserRole = editorUserRoleNames is null ? null : await _roleManager.FindByNameAsync(editorUserRoleNames.FirstOrDefault()!);
                    IList<Claim>? editorUserClaims = editorUserRole is null ? null : await _roleManager.GetClaimsAsync(editorUserRole);

                    //make sure that the added claims exist in the system
                    List<Claim> uniqueClaims = await GetAllUniqueClaimsInSystemAsyncCommonPart();
                    List<Claim> roleClaimsFiltered = new List<Claim>();
                    foreach (CustomClaim customRoleClaim in roleClaims)
                    {
                        //here make the check to see if the user can add this returnedClaim to the role
                        if (customRoleClaim.Value!.Contains("Elevated") &&
                        (editorUserClaims is null || !editorUserClaims.Any(editorClaim => editorClaim.Type == "Permission" && editorClaim.Value == "CanManageElevatedRoles")))
                            continue;

                        //if the check is passed then check here if the returnedClaim is contained in the database
                        if (uniqueClaims.Any(uniqueClaim => uniqueClaim.Type == customRoleClaim.Type && uniqueClaim.Value == customRoleClaim.Value))
                            roleClaimsFiltered.Add(new Claim(customRoleClaim.Type!, customRoleClaim.Value));
                    }

                    //check that there is not already another role with the given role name
                    var otherRole = await _roleManager.FindByNameAsync(roleName);
                    if (otherRole is not null)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3426, "CreateRoleFailureDueToDuplicateRole"), "There is already a role with the given RoleName={RoleName}.", roleName);
                        return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.DuplicateRole);
                    }

                    //create the role
                    var result = await _roleManager.CreateAsync(new AppRole(roleName));
                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3427, "CreateRoleFailureNoException"), "An error occured while trying to create the role, but no exception was thrown. RoleName={RoleName}. Errors={Errors}", roleName, result.Errors);
                        return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
                    }

                    AppRole? newlyCreatedAppRole = await _roleManager.FindByNameAsync(roleName);

                    //finally add the filtered roles to the role
                    foreach (Claim roleClaimFiltered in roleClaimsFiltered)
                    {
                        var addClaimResult = await _roleManager.AddClaimAsync(newlyCreatedAppRole!, roleClaimFiltered);
                        if (!addClaimResult.Succeeded)
                        {
                            await transaction.RollbackAsync();

                            _logger.LogWarning(new EventId(3428, "CreateRoleFailureInAddClaimsToRoleSubprocessNoException"), "An error occured while trying to add the given claims to the role role, but no exception was thrown. " +
                                "RoleName={RoleName}. Errors={Errors}", roleName, result.Errors);
                            return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);

                        }

                        newlyCreatedAppRole!.Claims.Add(new CustomClaim(roleClaimFiltered));
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation(new EventId(2400, "CreateRoleSuccess"), "The role with RoleName={RoleName} was sucessfully created.", roleName);
                    return new ReturnRoleAndCodeResponseModel(newlyCreatedAppRole!, LibraryReturnedCodes.NoError);
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(4404, "CreateRoleFailure"), ex, "An error occurred while trying to create role with RoleName={RoleName}. " +
                    "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", roleName, ex.Message, ex.StackTrace);

                    await transaction.RollbackAsync();
                    throw;
                }
            }
        });
    }

    public async Task<LibraryReturnedCodes> DeleteRoleAsync(string accessToken, List<Claim> expectedClaims, string roleId)
    {
        var executionStrategy = _appIdentityDbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _appIdentityDbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
                    ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3429, "DeleteRole"));
                    if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                        return returnCodeAndUserResponseModel.LibraryReturnedCodes;

                    var foundRole = await _roleManager.FindByIdAsync(roleId);
                    if (foundRole is null)
                    {
                        _logger.LogWarning(new EventId(3434, "DeleteRoleFailureDueToNullRole"), "Tried to delete null role. RoleId={RoleId}", roleId);
                        return LibraryReturnedCodes.RoleNotFoundWithGivenId;
                    }

                    LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundRole);
                    if (returnedCode != LibraryReturnedCodes.NoError)
                        return returnedCode;

                    if (foundRole.Name == "Admin" || foundRole.Name == "User")
                        return LibraryReturnedCodes.CanNotAlterFundementalRole;

                    var usersInRole = await _userManager.GetUsersInRoleAsync(foundRole.Name!);
                    if (usersInRole.Any())
                    {
                        foreach (var user in usersInRole)
                        {
                            IdentityResult addResult = await _userManager.AddToRoleAsync(user, "User");
                            if (!addResult.Succeeded)
                            {
                                _logger.LogWarning(new EventId(3435, "DeleteRoleFailureNoException"), "The delete role process return errors, but no exception was thrown. RoleId={RoleId}. Errors={Errors}", roleId, addResult.Errors);
                                return LibraryReturnedCodes.UnknownError;
                            }

                            IdentityResult removeResult = await _userManager.RemoveFromRoleAsync(user, foundRole.Name!);
                            if (!removeResult.Succeeded)
                            {
                                _logger.LogWarning(new EventId(3435, "DeleteRoleFailureNoException"), "The delete role process return errors, but no exception was thrown. RoleId={RoleId}. Errors={Errors}", roleId, removeResult.Errors);
                                return LibraryReturnedCodes.UnknownError;
                            }
                        }
                    }

                    var result = await _roleManager.DeleteAsync(foundRole);
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning(new EventId(3435, "DeleteRoleFailureNoException"), "The delete role process return errors, but no exception was thrown. RoleId={RoleId}. Errors={Errors}", roleId, result.Errors);
                        return LibraryReturnedCodes.UnknownError;
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation(new EventId(2401, "DeleteRoleSuccess"), "The role with RoleId={RoleId} was sucessfully deleted.", roleId);
                    return LibraryReturnedCodes.NoError;
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(4405, "DeleteFailure"), ex, "An error occurred while deleting the role with the RoleId={RoleId}. " +
                    "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", roleId, ex.Message, ex.StackTrace);
                    throw;
                }
            }
        });
    }

    public async Task<ReturnUsersAndCodeResponseModel> GetUsersOfGivenRoleAsync(string accessToken, List<Claim> expectedClaims, string roleId)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3436, "GetUsersOfGivenRole"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnUsersAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            var foundRole = await _roleManager.FindByIdAsync(roleId);
            if (foundRole is null)
            {
                _logger.LogWarning(new EventId(3441, "GetUsersOfGivenRoleFailureDueToNullRole"), "The role was not found in the system and thus no users could have it, RoleId={RoleId}", roleId);
                return new ReturnUsersAndCodeResponseModel(null!, LibraryReturnedCodes.RoleNotFoundWithGivenId);
            }

            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundRole);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return new ReturnUsersAndCodeResponseModel(null!, returnedCode);

            IList<AppUser> returnedUsersInRole = await _userManager.GetUsersInRoleAsync(foundRole.Name!);
            return new ReturnUsersAndCodeResponseModel(returnedUsersInRole.ToList(), LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4406, "GetUsersOfGivenRoleFailure"), ex, "An error occurred while retrieving the role with the RoleId={RoleId}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", roleId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibraryReturnedCodes> AddRoleToUserAsync(string accessToken, List<Claim> expectedClaims, string userId, string roleId)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3442, "AddRoleToUser"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel.LibraryReturnedCodes;

            var foundUser = await _userManager.FindByIdAsync(userId);
            if (foundUser is null)
            {
                _logger.LogWarning(new EventId(3447, "AddRoleToUserFailureDueToNullUser"), "Tried to add role to null user, UserId={UserId}, RoleId={RoleId}.", userId, roleId);
                return LibraryReturnedCodes.UserNotFoundWithGivenId;
            }

            var foundRole = await _roleManager.FindByIdAsync(roleId);
            if (foundRole is null)
            {
                _logger.LogWarning(new EventId(3448, "AddRoleToUserFailureDueToNullRole"), "role could not be found in system and thus could not be added to user. UserId={UserId}, RoleId={RoleId}.", userId, roleId);
                return LibraryReturnedCodes.RoleNotFoundWithGivenId;
            }

            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundRole);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return returnedCode;

            var result = await _userManager.AddToRoleAsync(foundUser, foundRole.Name!);
            if (!result.Succeeded && result.Errors.Any(error => error.Code == "UserAlreadyInRole"))
            {
                return LibraryReturnedCodes.UserWasFoundAlreadyInRole;
            }
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3449, "AddRoleToUserFailureNoException"), "The role could not be added to the user and returned errors, but no exception was thrown. " +
                    "RoleId={RoleId}. Errors={Errors}.", roleId, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(2402, "AddRoleToUserSuccess"), "The role with RoleId={RoleId} was sucessfully added to user with UserId={UserId}.", roleId, userId);
            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4407, "AddRoleToUserFailure"), ex, "The role could not be added to the user. RoleId={RoleId}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", roleId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibraryReturnedCodes> ReplaceRoleOfUserAsync(string accessToken, List<Claim> expectedClaims, string userId, string currentRoleId, string newRoleId)
    {
        var executionStrategy = _appIdentityDbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _appIdentityDbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken,
                        expectedClaims, new EventId(3450, "ReplaceRoleOfUser"));
                    if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                        return returnCodeAndUserResponseModel.LibraryReturnedCodes;

                    if (currentRoleId == newRoleId)
                        return LibraryReturnedCodes.TheRolesMustHaveDifferentIds;

                    var foundUser = await _userManager.FindByIdAsync(userId);
                    if (foundUser is null)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3455, "ReplaceRoleOfUserFailureDueToNullUser"), "Tried to replace role of null user. " +
                            "UserId={UserId}, CurrentRoleId={CurrentRoleId}, NewRoleId={NewRoleId}.", userId, currentRoleId, newRoleId);
                        return LibraryReturnedCodes.UserNotFoundWithGivenId;
                    }

                    var foundCurrentRole = await _roleManager.FindByIdAsync(currentRoleId);
                    if (foundCurrentRole is null)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3456, "ReplaceRoleOfUserFailureDueToNullOldRole"), "The current role was null and thus the process could not proceed, " +
                            "UserId={UserId}, CurrentRoleId={CurrentRoleId}, NewRoleId={NewRoleId}.", userId, currentRoleId, newRoleId);
                        return LibraryReturnedCodes.RoleNotFoundWithGivenId;
                    }

                    var foundNewRole = await _roleManager.FindByIdAsync(newRoleId);
                    if (foundNewRole is null)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3457, "ReplaceRoleOfUserFailureDueToNullRole"), "The new role was null and thus the process could not proceed, " +
                            "UserId={UserId}, CurrentRoleId={CurrentRoleId}, NewRoleId={NewRoleId}.", userId, currentRoleId, newRoleId);
                        return LibraryReturnedCodes.RoleNotFoundWithGivenId;
                    }

                    //check if the have the necessary permissions for the new role
                    LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundNewRole);
                    if (returnedCode != LibraryReturnedCodes.NoError)
                    {
                        await transaction.RollbackAsync();

                        return returnedCode;
                    }

                    //check if the have the necessary permissions for the current role
                    returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundCurrentRole);
                    if (returnedCode != LibraryReturnedCodes.NoError)
                    {
                        await transaction.RollbackAsync();

                        return returnedCode;
                    }

                    var result = await _userManager.RemoveFromRoleAsync(foundUser, foundCurrentRole.Name!);
                    if (!result.Succeeded && result.Errors.Any(error => error.Code == "UserNotInRole"))
                    {
                        await transaction.RollbackAsync();

                        return LibraryReturnedCodes.UserWasNotFoundInRole;
                    }
                    else if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3458, "ReplaceRoleOfUserFailureToRemoveCurrentRoleNoException"), "The process of removing the current role of the user failed and returned errors, but no exception was thrown. " +
                            "UserId={UserId}, CurrentRoleId={CurrentRoleId}, NewRoleId={NewRoleId}. Errors={Errors}", userId, currentRoleId, newRoleId, result.Errors);
                        return LibraryReturnedCodes.UnknownError;
                    }

                    result = await _userManager.AddToRoleAsync(foundUser, foundNewRole.Name!);
                    if (!result.Succeeded && result.Errors.Any(error => error.Code == "UserAlreadyInRole"))
                    {
                        await transaction.RollbackAsync();

                        return LibraryReturnedCodes.UserWasFoundAlreadyInRole;
                    }
                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3459, "ReplaceRoleOfUserFailureToAddNewRoleNoException"), "The process of adding the new role to the user failed and returned errors, but no exception was thrown. " +
                        "UserId={UserId}, CurrentRoleId={CurrentRoleId}, NewRoleId={NewRoleId}. Errors={Errors}", userId, currentRoleId, newRoleId, result.Errors);
                        return LibraryReturnedCodes.UnknownError;
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation(new EventId(2403, "ReplaceRoleOfUserSuccess"), "Successfully replaced role of user account. " +
                        "UserId={UserId}, CurrentRoleId={CurrentRole}, NewRoleId={NewRoleId}.", userId, currentRoleId, newRoleId);
                    return LibraryReturnedCodes.NoError;
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(4408, "ReplaceRoleOfUserFailure"), ex, "An error occurred while trying to replace the role of the user. UserId={UserId}, CurrentRoleId={CurrentRoleId}, NewRoleId={NewRoleId}. " +
                    "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, currentRoleId, newRoleId, ex.Message, ex.StackTrace);

                    await transaction.RollbackAsync();
                    throw;
                }
            }
        });
    }

    public async Task<LibraryReturnedCodes> RemoveRoleFromUserAsync(string accessToken, List<Claim> expectedClaims, string userId, string roleId)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3460, "RemoveRoleFromUser"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel.LibraryReturnedCodes;

            var foundUser = await _userManager.FindByIdAsync(userId);
            if (foundUser is null)
            {
                _logger.LogWarning(new EventId(3465, "RemoveRoleFromUserFailureDueToNullUser"), "Tried to remove role from null user, UserId={UserId}, RoleId={RoleId}.", userId, roleId);
                return LibraryReturnedCodes.UserNotFoundWithGivenId;
            }

            var foundRole = await _roleManager.FindByIdAsync(roleId);
            if (foundRole is null)
            {
                _logger.LogWarning(new EventId(3466, "RemoveRoleFromUserFailureDueToNullRole"), "role could not be found in system and thus could not be removed from the roles of the user. " +
                    "UserId={UserId}, RoleId={RoleId}.", userId, roleId);
                return LibraryReturnedCodes.RoleNotFoundWithGivenId;
            }

            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundRole);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return returnedCode;

            var result = await _userManager.RemoveFromRoleAsync(foundUser, foundRole.Name!);
            if (!result.Succeeded && result.Errors.Any(error => error.Code == "UserNotInRole"))
            {
                return LibraryReturnedCodes.UserWasNotFoundInRole;
            }
            else if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3467, "RemoveRoleFromUserFailureNoException"), "The role could not be removed from the user and returned errors, but no exception was thrown. " +
                    "RoleId={RoleId}. Errors={Errors}.", roleId, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(2404, "RemoveRoleFromUserSuccess"), "The role with RoleId={RoleId} was sucessfully removed from the roles of user with UserId={UserId}.", roleId, userId);
            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4409, "RemoveRoleFromUserFailure"), ex, "An error occurred while removing the given role from the user. UserId={UserId}, RoleId={RoleId}. " +
                    "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, roleId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnClaimsAndCodeResponseModel> GetAllUniqueClaimsInSystemAsync(string accessToken, List<Claim> expectedClaims)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken,
                expectedClaims, new EventId(3471, "GetAllUniqueClaimsInSystem"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnClaimsAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            List<Claim> uniqueClaims = await GetAllUniqueClaimsInSystemAsyncCommonPart();
            await FilterElevatedClaimsBasedOnUserPermissions(returnCodeAndUserResponseModel.AppUser!, uniqueClaims);

            //get the claims of the user that is trying to read the roles
            IList<string> editorUserRoleNames = await _userManager.GetRolesAsync(returnCodeAndUserResponseModel.AppUser!);
            AppRole? editorUserRole = editorUserRoleNames is null || editorUserRoleNames.Count == 0 ? null : await _roleManager.FindByNameAsync(editorUserRoleNames.FirstOrDefault()!);
            IList<Claim>? editorUserClaims = editorUserRole is null ? null : await _roleManager.GetClaimsAsync(editorUserRole);

            //if the user does not have elevated permissions then filter the claims that will be returned
            if (uniqueClaims is not null && (editorUserClaims is null || !editorUserClaims.Any(editorClaim => editorClaim.Type == "Permission" && editorClaim.Value == "CanManageElevatedRoles")))
                uniqueClaims.RemoveAll(claim => claim.Value!.Contains("Elevated"));

            List<CustomClaim> uniqueCustomClaims = new List<CustomClaim>();
            foreach (Claim uniqueClaim in uniqueClaims ?? Enumerable.Empty<Claim>())
                uniqueCustomClaims.Add(new CustomClaim(uniqueClaim));
            return new ReturnClaimsAndCodeResponseModel(uniqueCustomClaims!, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4410, "GetAllUniqueClaimsInSystemFailure"), ex, "An error occurred while retrieving the unique claims that exist in the system. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnRoleAndCodeResponseModel> UpdateClaimsOfRoleAsync(string accessToken, List<Claim> expectedClaims, string roleId, List<CustomClaim> updatedClaims)
    {
        var executionStrategy = _appIdentityDbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _appIdentityDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
                    ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken,
                        expectedClaims, new EventId(3476, "UpdateClaimsOfRole"));
                    if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                    {
                        await transaction.RollbackAsync();

                        return new ReturnRoleAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);
                    }

                    AppRole? foundRole = await _roleManager.FindByIdAsync(roleId);
                    if (foundRole is null)
                    {
                        await transaction.RollbackAsync();

                        _logger.LogWarning(new EventId(3481, "UpdateClaimsOfRoleFailureDueToNullRole"), "Tried to update claims of null role. RoleId={RoleId}", roleId);
                        return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.RoleNotFoundWithGivenId);
                    }

                    //Check if the role is fundemental 
                    if (foundRole.Name == "Admin" || foundRole.Name == "User")
                    {
                        await transaction.RollbackAsync();

                        return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.CanNotAlterFundementalRole);
                    }

                    LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificRole(returnCodeAndUserResponseModel.AppUser!, foundRole);
                    if (returnedCode != LibraryReturnedCodes.NoError)
                        return new ReturnRoleAndCodeResponseModel(null!, returnedCode);

                    List<Claim> uniqueClaims = await GetAllUniqueClaimsInSystemAsyncCommonPart();

                    List<Claim> filteredUpdatedClaims = new List<Claim>();
                    // This makes sure that the given claims actually exist in the system
                    foreach (CustomClaim updatedClaim in updatedClaims)
                    {
                        if (uniqueClaims.Any(claim => claim.Type == updatedClaim.Type && claim.Value == updatedClaim.Value))
                            filteredUpdatedClaims.Add(new Claim(updatedClaim.Type!, updatedClaim.Value!));
                    }

                    await FilterElevatedClaimsBasedOnUserPermissions(returnCodeAndUserResponseModel.AppUser!, filteredUpdatedClaims);

                    // Get the existing claims of the role
                    List<Claim> roleExistingClaims = new List<Claim>(await _roleManager.GetClaimsAsync(foundRole));

                    // Remove claims from the role that are in the filtered updated list of claims
                    foreach (Claim roleExistingClaim in roleExistingClaims)
                    {
                        if (filteredUpdatedClaims!.Any(filteredUpdatedClaim => filteredUpdatedClaim.Type == roleExistingClaim.Type && filteredUpdatedClaim.Value == roleExistingClaim.Value))
                            continue;

                        var removeClaimResult = await _roleManager.RemoveClaimAsync(foundRole, roleExistingClaim);
                        if (!removeClaimResult.Succeeded)
                        {
                            await transaction.RollbackAsync();

                            _logger.LogWarning(new EventId(3482, "UpdateClaimsOfRoleRemoveClaimsSubprocessFailureNoException"), "Trying to remove claims from role returned errors, but no exception was thrown. " +
                                "RoleId={RoleId}. Errors={Errors}.", roleId, removeClaimResult.Errors);
                            return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
                        }
                    }

                    // Add claims that are not already part of the role
                    foreach (Claim filteredUpdatedClaim in filteredUpdatedClaims)
                    {
                        if (roleExistingClaims.Any(roleExistingClaim => roleExistingClaim.Type == filteredUpdatedClaim.Type && roleExistingClaim.Value == filteredUpdatedClaim.Value))
                            continue;

                        var addClaimResult = await _roleManager.AddClaimAsync(foundRole, filteredUpdatedClaim);
                        if (!addClaimResult.Succeeded)
                        {
                            await transaction.RollbackAsync();

                            _logger.LogWarning(new EventId(3483, "UpdateClaimsOfRoleRemoveClaimsSubprocessFailureNoException"), "Trying to add claims to role returned errors, but no exception was thrown. " +
                                "RoleId={RoleId}. Errors={Errors}.", roleId, addClaimResult.Errors);

                            return new ReturnRoleAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
                        }
                    }

                    IList<Claim> outputClaims = await _roleManager.GetClaimsAsync(foundRole);
                    foreach (Claim outputClaim in outputClaims ?? Enumerable.Empty<Claim>())
                        foundRole.Claims.Add(new CustomClaim(outputClaim));

                    await transaction.CommitAsync();

                    _logger.LogInformation(new EventId(2405, "UpdateClaimsOfRoleSuccess"), "The claims of the role with RoleId={RoleId} were sucessfully updated.", roleId);
                    return new ReturnRoleAndCodeResponseModel(foundRole, LibraryReturnedCodes.NoError);
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(4411, "UpdateClaimsOfRoleFailure"), ex, "An error occurred while trying to update the claims of the Role with RoleId={RoleId}. " +
                    "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", roleId, ex.Message, ex.StackTrace);

                    await transaction.RollbackAsync();
                    throw;
                }
            }
        });
    }

    private async Task<List<Claim>> GetAllUniqueClaimsInSystemAsyncCommonPart()
    {
        List<AppRole> appRoles = await _roleManager.Roles.ToListAsync();
        List<Claim> uniqueClaims = new List<Claim>();

        //Claims in AspNetCore Identity are terribly implemented using one to many. That means that a role can have many claims, but a customRoleClaim can not have many roles.
        //Terrible implementation, but disregarding that the following needs to happen, because there might exist 2 claims that have the same customRoleClaim type and value, but different claimId and claimrole
        foreach (AppRole appRole in appRoles)
        {
            List<Claim> returnedRoleClaims = new List<Claim>(await _roleManager.GetClaimsAsync(appRole));
            foreach (Claim returnedRoleClaim in returnedRoleClaims)
            {
                if (!uniqueClaims.Any(uniqueClaim => uniqueClaim.Type == returnedRoleClaim.Type && uniqueClaim.Value == returnedRoleClaim.Value))
                    uniqueClaims.Add(returnedRoleClaim);
            }
        }

        return uniqueClaims;
    }

    private async Task FilterElevatedClaimsBasedOnUserPermissions(AppUser appUser, List<Claim> claims)
    {
        //get the claims of the user that is trying to read the roles
        IList<string> editorUserRoleNames = await _userManager.GetRolesAsync(appUser!);
        AppRole? editorUserRole = editorUserRoleNames is null || editorUserRoleNames.Count == 0 ? null : await _roleManager.FindByNameAsync(editorUserRoleNames.FirstOrDefault()!);
        IList<Claim>? editorUserClaims = editorUserRole is null ? null : await _roleManager.GetClaimsAsync(editorUserRole);

        //if the user does not have elevated permissions then filter the claims that will be returned
        if (claims is not null && (editorUserClaims is null || !editorUserClaims.Any(editorClaim => editorClaim.Type == "Permission" && editorClaim.Value == "CanManageElevatedRoles")))
            claims.RemoveAll(claim => claim.Value!.Contains("Elevated"));
    }
}
