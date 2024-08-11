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
public class AdminProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<AuthenticationProcedures> _logger;
    private readonly IHelperMethods _helperMethods;
    private readonly AppIdentityDbContext _identityDbContext;

    public AdminProcedures(UserManager<AppUser> userManager, AppIdentityDbContext appIdentityDbContext, IHelperMethods helperMethods, ILogger<AuthenticationProcedures> logger = null!)
    {
        _identityDbContext = appIdentityDbContext;
        _userManager = userManager;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
        _helperMethods = helperMethods;
    }

    public async Task<ReturnUsersAndCodeResponseModel> GetUsersAsync(string accessToken, List<Claim> expectedClaims)
    {
        try
        {
            //checks for token corresponding to a user in the system, for the user account to be confirmed and for the user account to not be locked out.
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3300, "GetUsers"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnUsersAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            var appUsers = await _userManager.Users.ToListAsync();
            return new ReturnUsersAndCodeResponseModel(appUsers, LibraryReturnedCodes.NoError);
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

            AppUser? appUser = await _userManager.FindByIdAsync(userId);
            return new ReturnUserAndCodeResponseModel(appUser!, LibraryReturnedCodes.NoError);
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


            AppUser? appUser = await _userManager.FindByEmailAsync(email);
            return new ReturnUserAndCodeResponseModel(appUser!, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4302, "FindByEmailFailure"), ex, "An error occurred while retrieving user with Email={Email}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnUserAndCodeResponseModel> CreateUserAccountAsync(string accessToken, List<Claim> expectedClaims, string email, string phoneNumber, string password)
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
                        return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.ValidTokenButUserNotInSystem);
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

                    // If currentAppUser creation fails, rollback the transaction
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

    public async Task<LibraryReturnedCodes> UpdateUserAccountAsync(string accessToken, List<Claim> expectedClaims, AppUser updatedUser, string? password = null, bool activateEmail = true)
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
                        return LibraryReturnedCodes.ValidTokenButUserNotInSystem;
                    }

                    //check if the user 
                    AppUser? currentAppUser = await _userManager.FindByIdAsync(updatedUser.Id);
                    if (currentAppUser is null)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning(new EventId(3328, "UpdateUserAccountFailureDueToNullUser"), "Tried to update null user. UserId={UserId}", updatedUser.Id);
                        return LibraryReturnedCodes.UserNotFoundWithGivenId;
                    }

                    //check if another user has the given email
                    if (updatedUser.Email is not null && updatedUser.Email != currentAppUser.Email)
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
                        bool checkPasswordResult = await _userManager.CheckPasswordAsync(currentAppUser, password);
                        if (!checkPasswordResult)
                        {
                            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(currentAppUser);
                            var changePasswordResult = await _userManager.ResetPasswordAsync(currentAppUser, passwordResetToken, password);
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
                        currentAppUser.PhoneNumber = updatedUser.PhoneNumber.Trim() == "" ? null : updatedUser.PhoneNumber.Trim();

                    currentAppUser.Email = updatedUser.Email is null ? currentAppUser.Email : updatedUser.Email;
                    currentAppUser.UserName = updatedUser.Email is null ? currentAppUser.Email : updatedUser.Email; //The email and the username are the same
                    currentAppUser.EmailConfirmed = activateEmail;
                    var updateBasicFieldsResult = await _userManager.UpdateAsync(currentAppUser);
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


            AppUser? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser is null)
            {
                _logger.LogWarning(new EventId(3337, "DeleteUserAccountFailureDueToNullUser"), "Tried to delete email of null user. UserId={UserId}", userId);
                return LibraryReturnedCodes.UserNotFoundWithGivenId;
            }

            var result = await _userManager.DeleteAsync(appUser);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3338, "DeleteUserAccountFailureNoException"), "User account could not be deleted, but no exception was thrown. UserId={UserId}. Errors={Errors}.", appUser.Email, result.Errors);
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
