using EshopApp.AuthLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using EshopApp.AuthLibrary.Models.ResponseModels;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.AuthLogic;

//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 300-399, for example 2300 or 2399.
//TODO figure a better way to do logging ids
public class AdminProcedures : IAdminProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly ILogger<AuthenticationProcedures> _logger;
    private readonly IHelperMethods _helperMethods;
    private readonly AppIdentityDbContext _identityDbContext;

    public AdminProcedures(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppIdentityDbContext appIdentityDbContext, IHelperMethods helperMethods, ILogger<AuthenticationProcedures> logger = null!)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _identityDbContext = appIdentityDbContext;
        _helperMethods = helperMethods;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
    }

    public async Task<ReturnUsersAndCodeResponseModel> GetUsersAsync(string accessToken, List<Claim> expectedClaims)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3300, "GetUsers"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnUsersAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            var foundUsers = await _userManager.Users.ToListAsync();

            IList<string> editorUserRoleNames = await _userManager.GetRolesAsync(returnCodeAndUserResponseModel.AppUser!);
            AppRole? editorUserRole = editorUserRoleNames is null || editorUserRoleNames.Count == 0 ? null : await _roleManager.FindByNameAsync(editorUserRoleNames!.FirstOrDefault()!);
            IList<Claim>? editorUserClaims = editorUserRole is null ? null : await _roleManager.GetClaimsAsync(editorUserRole);

            //if the user that called the endpoint has elevated access just return all users
            if(editorUserClaims is not null && editorUserClaims.Any(claim => claim.Type == "Permission" && claim.Value == "CanManageElevatedUsers"))
                return new ReturnUsersAndCodeResponseModel(foundUsers, LibraryReturnedCodes.NoError);

            //otherwise just return users who do not have elevated protections
            var filteredFoundUsers = new List<AppUser>();
            foreach (AppUser foundUser in foundUsers)
            {
                IList<string> editedUserRoleNames = await _userManager.GetRolesAsync(foundUser);
                AppRole? editedUserRole = editedUserRoleNames is null || editedUserRoleNames.Count == 0 ? null : await _roleManager.FindByNameAsync(editedUserRoleNames.FirstOrDefault()!);
                IList<Claim>? editedUserClaims = editedUserRole is null ? null : await _roleManager.GetClaimsAsync(editedUserRole);

                if (editedUserClaims is null || !editedUserClaims.Any(claim => claim.Type == "Protection" && claim.Value == "CanOnlyBeManagedByElevatedUsers"))
                    filteredFoundUsers.Add(foundUser);
            }
            
            return new ReturnUsersAndCodeResponseModel(filteredFoundUsers, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4300, "GetUsersFailure"), ex, "An error occurred while retrieving users. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnUserAndCodeResponseModel?> FindUserByIdAsync(string accessToken, List<Claim> expectedClaims, string userId)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3305, "FindUserById"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel; //null and the error status code

            AppUser? foundUser = await _userManager.FindByIdAsync(userId);
            if (foundUser is null)
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.NoError);

            //check if the user does not have the highest priviledges. If they dont check to see if they try to update a user with the highest priviledges
            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificUser(returnCodeAndUserResponseModel.AppUser!, foundUser!);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.InsufficientPrivilegesToManageElevatedUser);

            return new ReturnUserAndCodeResponseModel(foundUser!, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4301, "GetUserByIdFailure"), ex, "An error occurred while retrieving user with UserId={UserId}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnUserAndCodeResponseModel?> FindUserByEmailAsync(string accessToken, List<Claim> expectedClaims, string email)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3310, "FindUserByEmail"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel; //null and the error status code

            AppUser? foundUser = await _userManager.FindByEmailAsync(email);
            if (foundUser is null)
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.NoError);

            //check if the user does not have the highest priviledges. If they dont check to see if they try to update a user with the highest priviledges
            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificUser(returnCodeAndUserResponseModel.AppUser!, foundUser!);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.InsufficientPrivilegesToManageElevatedUser);

            return new ReturnUserAndCodeResponseModel(foundUser!, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4302, "FindByEmailFailure"), ex, "An error occurred while retrieving user with Email={Email}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnUserAndCodeResponseModel> CreateUserAccountAsync(string accessToken, List<Claim> expectedClaims, string email, string password, string? phoneNumber = null)
    {

        var executionStrategy = _identityDbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _identityDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
                    ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3315, "CreateUserAccount"));
                    if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                    {
                        await transaction.RollbackAsync();
                        return new ReturnUserAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);
                    }

                    AppUser? otherUser = await _userManager.FindByEmailAsync(email);
                    if (otherUser is not null)
                    {

                        await transaction.RollbackAsync();
                        _logger.LogWarning(new EventId(3320, "CreateUserAccountFailureDueToDuplicateEmail"), "Another user has the given email and thus the create account process can not proceed. Email={Email}.", email);
                        return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.DuplicateEmail);
                    }

                    var appUser = new AppUser();
                    appUser.Id = Guid.NewGuid().ToString();

                    // make sure that the guid is unique(extreme edge case)
                    otherUser = await _userManager.FindByIdAsync(appUser.Id);
                    while (otherUser is not null)
                        appUser.Id = Guid.NewGuid().ToString();

                    appUser.UserName = email;
                    appUser.Email = email;
                    appUser.PhoneNumber = phoneNumber;

                    var result = await _userManager.CreateAsync(appUser, password);

                    // If userToBeUpdated creation fails, rollback the transaction
                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(new EventId(3321, "CreateUserAccountFailureUnknownError"), "An error occurred while creating user account, but an exception was not thrown. Email={Email}, Errors={Errors}.",
                            appUser.Email, result.Errors);

                        return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
                    }

                    // Confirm the email
                    appUser.EmailConfirmed = true;
                    result = await _userManager.UpdateAsync(appUser);
                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(new EventId(3322, "CreateUserAccountFailureUnknownError"), "An error occurred while creating user account, but an exception was not thrown. Email={Email}, Errors={Errors}.",
                            appUser.Email, result.Errors);

                        return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
                    }

                    await _userManager.AddToRoleAsync(appUser, "User"); //for now just add the user in the role user

                    // If everything is successful, commit the transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation(new EventId(2300, "CreateUserAccountSuccess"), "The user account with UserId={UserId} was sucessfully created.", appUser.Id);


                    return new ReturnUserAndCodeResponseModel(appUser, LibraryReturnedCodes.NoError);
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(4303, "CreateUserAccountFailure"), ex, "An error occurred while creating user account. " +
                        "Email={Email}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", email, ex.Message, ex.StackTrace);

                    // If an exception occurs, rollback the transaction
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        });
    }

    public async Task<LibraryReturnedCodes> UpdateUserAccountAsync(string accessToken, List<Claim> expectedClaims, AppUser updatedUser, bool activateEmail, string? password = null)
    {
        var executionStrategy = _identityDbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            using (var transaction = await _identityDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    ReturnUserAndCodeResponseModel standardTokenProceduresTestModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken,
                        expectedClaims, new EventId(3323, "UpdateUserAccount"));
                    if (standardTokenProceduresTestModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                    {
                        await transaction.RollbackAsync();
                        return standardTokenProceduresTestModel.LibraryReturnedCodes;
                    }

                    //check if the user 
                    AppUser? userToBeUpdated = await _userManager.FindByIdAsync(updatedUser.Id);
                    if (userToBeUpdated is null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(new EventId(3328, "UpdateUserAccountFailureDueToNullUser"), "Tried to update null user. UserId={UserId}", updatedUser.Id);
                        return LibraryReturnedCodes.UserNotFoundWithGivenId;
                    }

                    //check if the user does not have the highest priviledges. If they dont check to see if they try to update a user with the highest priviledges
                    LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificUser(standardTokenProceduresTestModel.AppUser!, userToBeUpdated);
                    if (returnedCode != LibraryReturnedCodes.NoError)
                        return returnedCode;

                    //check if another user has the given email
                    if (updatedUser.Email is not null && updatedUser.Email != userToBeUpdated.Email)
                    {
                        AppUser? otherUser = await _userManager.FindByEmailAsync(updatedUser.Email);
                        if (otherUser is not null)
                        {
                            await transaction.RollbackAsync();
                            _logger.LogWarning(new EventId(3329, "UpdateUserAccountFailureDueToDuplicateEmail"), "Another user has the given email and thus the update user account process could not proceed. " +
                                "Email={Email}.", updatedUser.Email);
                            return LibraryReturnedCodes.DuplicateEmail;
                        }
                    }

                    //password update if a new password is provided
                    if (password is not null)
                    {
                        bool checkPasswordResult = await _userManager.CheckPasswordAsync(userToBeUpdated, password);
                        if (!checkPasswordResult)
                        {
                            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(userToBeUpdated);
                            var changePasswordResult = await _userManager.ResetPasswordAsync(userToBeUpdated, passwordResetToken, password);
                            if (!changePasswordResult.Succeeded)
                            {
                                await transaction.RollbackAsync();
                                _logger.LogWarning(new EventId(3330, "UpdateUserAccountDueToFailureInThePasswordChange"), "The password subprocess of the update user account process returned errors, but no exception was thrown. " +
                                    "UserId={UserId}. Errors={Errors}.", updatedUser.Id, changePasswordResult.Errors);
                                return LibraryReturnedCodes.UnknownError;
                            }
                        }
                    }

                    //update basic fields like phoneNuber
                    if (updatedUser.PhoneNumber is not null)
                        userToBeUpdated.PhoneNumber = updatedUser.PhoneNumber.Trim() == "" ? null : updatedUser.PhoneNumber.Trim();

                    userToBeUpdated.Email = updatedUser.Email is null ? userToBeUpdated.Email : updatedUser.Email;
                    userToBeUpdated.UserName = updatedUser.Email is null ? userToBeUpdated.Email : updatedUser.Email; //The email and the username are the same
                    userToBeUpdated.EmailConfirmed = activateEmail;
                    var updateBasicFieldsResult = await _userManager.UpdateAsync(userToBeUpdated);
                    if (!updateBasicFieldsResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(new EventId(3331, "UpdateUserAccountFailureDueToUnknownError"),
                            "The general user update part that updates most of the fields of the user returned errors: UserId={UserId}. Errors={Errors}.", updatedUser.Id, updateBasicFieldsResult.Errors);
                        return LibraryReturnedCodes.UnknownError;
                    }

                    // If everything is successful, commit the transaction
                    await transaction.CommitAsync();
                    _logger.LogInformation(new EventId(2301, "UpdateUserAccountSuccess"), "The user account with UserId={UserId} was sucessfully updated.", updatedUser.Id);
                    return LibraryReturnedCodes.NoError;
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(4304, "UpdateUserAccountFailure"), ex, "An error occurred while updating a user account. " +
                        "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);

                    // If an exception occurs, rollback the transaction
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        });
    }

    public async Task<LibraryReturnedCodes> DeleteUserAccountAsync(string accessToken, List<Claim> expectedClaims, string userId)
    {
        try
        {
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3332, "DeleteUserAccount"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel.LibraryReturnedCodes; //null and the error status code


            AppUser? userToBeDeleted = await _userManager.FindByIdAsync(userId);
            if (userToBeDeleted is null)
            {
                _logger.LogWarning(new EventId(3337, "DeleteUserAccountFailureDueToNullUser"), "Tried to delete email of null user. UserId={UserId}", userId);
                return LibraryReturnedCodes.UserNotFoundWithGivenId;
            }

            //check if the user does not have the highest priviledges. If they dont check to see if they try to update a user with the highest priviledges
            LibraryReturnedCodes returnedCode = await _helperMethods.CheckIfAuthorizedToEditSpecificUser(returnCodeAndUserResponseModel.AppUser!, userToBeDeleted);
            if (returnedCode != LibraryReturnedCodes.NoError)
                return returnedCode;

            var result = await _userManager.DeleteAsync(userToBeDeleted);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3338, "DeleteUserAccountFailureNoException"), "User account could not be deleted, but no exception was thrown. UserId={UserId}. Errors={Errors}.", userToBeDeleted.Email, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(2302, "DeleteUserAccountSuccess"), "Successfully deleted user account. UserId={UserId}, Errors={Errors}", userId, result.Errors);

            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4305, "DeleteUserAccountFailure"), ex, "An error occurred while trying to delete the user account. " +
                "UserId={UserId}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
