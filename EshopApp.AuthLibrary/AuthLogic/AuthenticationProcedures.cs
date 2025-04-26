using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationModels;
using EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationProceduresModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EshopApp.AuthLibrary.AuthLogic;


//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 200-299, for example 2200 or 2299.
public class AuthenticationProcedures : IAuthenticationProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthenticationProcedures> _logger;
    private readonly IConfiguration _config;
    private readonly AppIdentityDbContext _identityDbContext;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly IHelperMethods _helperMethods;
    public AuthenticationProcedures(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppIdentityDbContext appIdentityDbContext, RoleManager<AppRole> roleManager,
        IConfiguration config, IHelperMethods helperMethods, ILogger<AuthenticationProcedures> logger = null!)
    {
        _identityDbContext = appIdentityDbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
        _helperMethods = helperMethods;
        _config = config;
    }

    public async Task<ReturnUserAndCodeResponseModel> GetCurrentUserByTokenAsync(string accessToken)
    {
        try
        {
            return await _helperMethods.StandardTokenAndUserValidationProcedures(accessToken, new EventId(3200, "GetCurrentUserByToken"));
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4200, "CurrentUserCouldNotBeRetrieved"), ex, "An error occurred while retrieving logged in user. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibSignUpResponseModel> SignUpAsync(string email, string phoneNumber, string password)
    {
        try
        {
            AppUser? appUser = await _userManager.FindByEmailAsync(email);
            if (appUser is not null)
            {
                _logger.LogWarning(new EventId(3203, "SignUpFailureDueToDuplicateEmail"), "Another user has the given email and thus the signup process can not proceed. Email={Email}.", email);
                return new LibSignUpResponseModel(null!, null!, LibraryReturnedCodes.DuplicateEmail);
            }

            appUser = new AppUser();
            appUser.Id = Guid.NewGuid().ToString();

            //make sure that the guid is unique(extreme edge case)
            AppUser? otherUser = await _userManager.FindByIdAsync(appUser.Id);
            while (otherUser is not null)
                appUser.Id = Guid.NewGuid().ToString();

            appUser.UserName = email;
            appUser.Email = email;
            appUser.PhoneNumber = phoneNumber;

            var result = await _userManager.CreateAsync(appUser, password);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3204, "SignUpFailureUnknownError"), "An error occurred while creating user account, but an exception was not thrown. Email={Email}, Errors={Errors}.",
                    appUser.Email, result.Errors);
                return new LibSignUpResponseModel(null!, null!, LibraryReturnedCodes.UnknownError);
            }

            await _userManager.AddToRoleAsync(appUser, "User");

            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            _logger.LogInformation(new EventId(2200, "SignUpSuccess"), "Successfully created user account: UserId={UserId}, Email={Email}.", appUser.Id, appUser.Email);

            return new LibSignUpResponseModel(confirmationToken, appUser.Id, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4201, "SignUpFailure"), ex, "An error occurred while creating user account. " +
                "Email:{Email}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnTokenAndCodeResponseModel> ConfirmEmailAsync(string userId, string confirmationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3205, "EmailConfirmationFailureDueToNullUser"), "Tried to confirm email of null user." +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}.", userId, confirmationToken);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3206, "EmailConfirmationFailureNoException"), "Email of user could not be confirmed." +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}. Errors={Errors}.", userId, confirmationToken, result.Errors);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
            }

            string accessToken = await GenerateTokenAsync(user);

            _logger.LogInformation(new EventId(2201, "UserEmailSuccessfullyConfirmed"), "Successfully confirmed user's email account. UserId={UserId}", userId);
            return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4202, "EmailConfirmationFailure"), ex, "An error occurred while confirming user email account. " +
                "UserId={UserId}, ConfirmationToken={ConfirmationToken}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, confirmationToken, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibraryReturnedCodes> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword)
    {
        try
        {
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenAndUserValidationProcedures(accessToken, new EventId(3207, "ChangePassword"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel.LibraryReturnedCodes;

            AppUser appUser = returnCodeAndUserResponseModel.AppUser!;

            IdentityResult result;
            if (currentPassword is not null)
                result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
            //this can happen if the updatedAppUser created an account through an external identity provider(edge case)
            else
                result = await _userManager.AddPasswordAsync(appUser, newPassword);

            if (!result.Succeeded && result.Errors.Where(error => error.Code == "PasswordMismatch").Count() > 0)
                return LibraryReturnedCodes.PasswordMissmatch;
            else if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3210, "PasswordChangeFailureNoException"), "Password could not be changed= UserId={UserId}, Email={Email}. Errors={Errors}.", appUser.Id, appUser.Email, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(2202, "UserPasswordSuccessfullyChanged"), "Successfully changed user's account password. UserId={UserId}, Email={Email}.", appUser.Id, appUser.Email);
            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4203, "PasswordChangeFailure"), ex, "An error occurred while changing user account password. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            return LibraryReturnedCodes.UnknownError;
        }
    }

    public async Task<ReturnTokenAndCodeResponseModel> SignInAsync(string email, string password, bool isPersistent)
    {
        try
        {
            AppUser? user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3211, "SignInFailureDueToNullUser"), "Tried to sign in null user= Email={Email}.", email);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenEmail);
            }

            if (!_helperMethods.IsEmailConfirmed(user, new EventId(3212, "SignInFailureDueToUnconfirmedEmail"), "The sign in process could not continue, because the user account is not activated. Email= {Email}."))
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);


            if (await _helperMethods.IsAccountLockedOut(user, new EventId(3213, "SignInFailureDueToAccountBeingLocked"), "User with locked account tried to sign in. Email={Email}."))
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountLocked);

            var result = await _userManager.CheckPasswordAsync(user!, password)!;
            if (!result)
            {
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.GetAccessFailedCountAsync(user) >= 10)
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(TimeSpan.FromMinutes(10)));

                _logger.LogWarning(new EventId(3214, "UserSignInFailureDueToInvalidCredentials"), "User could not be signed in, because of invalid credentials. Email={Email}.", email);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.InvalidCredentials);
            }

            //Get all the roles of the user
            List<string> userRoles = new List<string>(await _userManager.GetRolesAsync(user));
            string accessToken = await GenerateTokenAsync(user!, isPersistent, userRoles);
            _logger.LogInformation(new EventId(2203, "UserSignInSuccess"), "Successfully signed in user. Username={Email}, IsPersistent={IsPersistent}.", email, isPersistent);

            return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4204, "UserSignInFailure"), ex, "An error occurred while trying to sign in the user. " +
                "Email={Email}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<AuthenticationScheme>> GetExternalIdentityProvidersAsync()
    {
        try
        {
            return await _signInManager.GetExternalAuthenticationSchemesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4205, "ExternalIdentityProvidersRetrievalFailure"), ex, "An error occurred while trying to get the external identity providers. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public AuthenticationProperties GetExternalIdentityProvidersProperties(string identityProviderName, string redirectUrl)
    {
        try
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(identityProviderName, redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4206, "ExternalIdentityProviderPropertiesRetrievalFailure"), ex, "An error occurred while trying to get the external identity provider's properties. " +
                "IdentityProviderName= {IdentityProviderName}, RedirectUrl= {RedirectUrl}. ExceptionMessage{ExceptionMessage}. StackTrace={StackTrace}.", identityProviderName, redirectUrl, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnTokenAndCodeResponseModel> HandleExternalSignInCallbackAsync()
    {
        try
        {
            ExternalLoginInfo? loginInfo = await _signInManager.GetExternalLoginInfoAsync();
            string? accessToken = null;
            if (loginInfo is null)
            {
                _logger.LogWarning(new EventId(3215, "ExternalSignInFailureDueToNullLoginInfo"), "login info of external identity provider was not received.");
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.LoginInfoNotReceivedFromIdentityProvider);
            }

            string email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)!;
            string username = email;
            //if the returned information does not contain email give up
            if (email is null)
            {
                _logger.LogWarning(new EventId(3216, "ExternalSignInFailureBecauseEmailClaimIsMissing"), "External Sign in failure, because the email claim of the user was not received from the external identity provider.");
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.EmailClaimNotReceivedFromIdentityProvider);
            }

            //check for a lockout
            AppUser? user = await _userManager.FindByEmailAsync(email);
            //if the updatedAppUser does not have a local account and also not an external login create the local account and connect it with the incoming external login
            if (user is null)
            {
                //make sure that the guid is unique(extreme edge case)
                string guid = Guid.NewGuid().ToString();
                AppUser? otherUser = await _userManager.FindByIdAsync(guid);
                while (otherUser is not null)
                    guid = Guid.NewGuid().ToString();

                user = new AppUser() { Id = guid, UserName = username, Email = email, EmailConfirmed = true };

                await _userManager.CreateAsync(user);
                await _userManager.AddLoginAsync(user, loginInfo);
                accessToken = await GenerateTokenAsync(user!);

                _logger.LogInformation(new EventId(2204, "SuccessfulLoginAndCreationOfLocalAccountWithExternalLogin"), "Successfully created a local account linked the external login to it and signed the user in. Email={Email}.", email);
                return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
            }

            //if the updatedAppUser has a local a local account
            //check to see if the local account is activated
            if (!_helperMethods.IsEmailConfirmed(user, new EventId(3217, "ExternalSignInFailureDueToUnconfirmedEmail"), "The external sign in process could not continue, because the user account is not activated. Email={Email}"))
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

            //check to see if the local account is locked out
            if (await _helperMethods.IsAccountLockedOut(user, new EventId(3218, "ExternalSignInFailureDueToAccountBeingLocked"), "User with locked account tried to externally sign in= Email={Email}."))
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountLocked);

            //Get all the roles of the user
            List<string> userRoles = new List<string>(await _userManager.GetRolesAsync(user));

            //Try to see if a local account and an external login already exist by trying to loging 
            if (await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey) is not null)
            {

                //then if both exist, try to login using that external login
                var externalLoginSignInResult = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false, bypassTwoFactor: false);
                if (!externalLoginSignInResult.Succeeded)
                {
                    _logger.LogWarning(new EventId(3219, "ExternalLoginFailureNoException"), "The attempt to sign in using the external login for the given local account returned error, but there was no exception. Email={Email}.", email);
                    return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
                }

                _logger.LogInformation(new EventId(2205, "SuccessfulExternalUserSignIn"), "Successfully signed in user with external login provider.");
                accessToken = await GenerateTokenAsync(user!, false, userRoles);
                return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
            };

            //Finally if the updatedAppUser has already a local account and not an external login, try to connect the incoming external login with it
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                _logger.LogWarning(new EventId(3220, "ExternalLoginAndLocalAccountLinkFailureNoException"), "The external login could not be added to the account, but there was no exception. Email={Email}.", email);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
            }

            _logger.LogInformation(new EventId(2206, "SuccessfulExternalLoginAndLocalAccountLink"), "Successfully linked external login to user local account and signed them in. Email={Email}.", email);
            accessToken = await GenerateTokenAsync(user!, false, userRoles);
            return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4207, "ExternalUserSignFailure"), ex, "An error occurred while trying to sign in the user with external identity provider. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibraryReturnedCodes> DeleteAccountAsync(string accessToken)
    {
        try
        {
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenAndUserValidationProcedures(accessToken, new EventId(3221, "DeleteAccount"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel.LibraryReturnedCodes;

            AppUser appUser = returnCodeAndUserResponseModel.AppUser!;

            var result = await _userManager.DeleteAsync(appUser);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3224, "DeleteAccountFailureNoException"), "User account could not be deleted, but no exception was thrown. Email={Email}. Errors={Errors}.", appUser.Email, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(2207, "DeleteAccountSuccess"), "Successfully deleted user account. Email={Email}", appUser.Email);

            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4208, "DeleteAccountFailure"), ex, "An error occurred while trying to delete the user account. " +
                "AccessToken={AccessToken}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", accessToken, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnTokenUserIdAndCodeResponseModel> CreateResetPasswordTokenAsync(string email)
    {
        try
        {
            AppUser? appUser = await _userManager.FindByEmailAsync(email!);

            if (appUser is null)
            {
                _logger.LogWarning(new EventId(3225, "CreateResetPasswordTokenFailureDueToNullUser"), "Tried to create reset password token for null user. Email={Email}.", email);
                return new ReturnTokenUserIdAndCodeResponseModel(null!, null!, LibraryReturnedCodes.UserNotFoundWithGivenEmail);
            }

            if (!_helperMethods.IsEmailConfirmed(appUser, new EventId(3226, "CreateResetPasswordPasswordFailureDueToUnconfirmedEmail"), "The create reset password token process could not continue, because the user account is not activated. Email= {Email}"))
                return new ReturnTokenUserIdAndCodeResponseModel(null!, null!, LibraryReturnedCodes.UserAccountNotActivated);

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            _logger.LogInformation(new EventId(2208, "UserResetTokenCreationSuccess"), "Successfully created password reset token. UserId={UserId}, Email={Email}.", appUser.Id, appUser.Email);

            return new ReturnTokenUserIdAndCodeResponseModel(passwordResetToken, appUser.Id, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4209, "UserResetTokenCreationFailure"), ex, "An error occurred while trying to create reset account password token. " +
                "Email={Email}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnTokenAndCodeResponseModel> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3227, "UserPasswordResetFailureDueToNullUser"), "Tried to reset account password of null user. UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.", userId, resetPasswordToken);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
            }

            if (!_helperMethods.IsEmailConfirmed(user, new EventId(3228, "ResetPasswordFailureDueToUnconfirmedEmail"), "The reset password process could not continue, because the user account is not activated. Email= {Email}"))
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3218, "UserPasswordResetFailureNoException"), "User account password could not be reset. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}. Errors={Errors}.", userId, resetPasswordToken, result.Errors);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
            }

            //Get all the roles of the user
            List<string> userRoles = new List<string>(await _userManager.GetRolesAsync(user));
            string accessToken = await GenerateTokenAsync(user, false, userRoles);
            _logger.LogInformation(new EventId(2209, "UserPasswordResetSuccess"), "Successfully reset account password. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.", userId, resetPasswordToken);

            return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4210, "UserPasswordResetFailure"), ex, "An error occurred while trying reset user's account password. " +
                "UserId={UserId}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnTokenAndCodeResponseModel> CreateChangeEmailTokenAsync(string accessToken, string newEmail)
    {
        try
        {
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenAndUserValidationProcedures(accessToken, new EventId(9996, "CreateChangeEmailToken"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return new ReturnTokenAndCodeResponseModel(null!, returnCodeAndUserResponseModel.LibraryReturnedCodes);

            AppUser appUser = returnCodeAndUserResponseModel.AppUser!;

            AppUser? otherUser = await _userManager.FindByEmailAsync(newEmail!);
            if (otherUser is not null)
            {
                _logger.LogWarning(new EventId(3229, "EmailChangeTokenCreationFailureDueToDuplicateEmail"), "Another user has the given new email and thus the process of creating email change token could not be completed. " +
                    "NewEmail={NewEmail}.", newEmail);
                return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.DuplicateEmail);
            }

            string emailChangeToken = await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);

            appUser.EmailConfirmed = false;
            var result = await _userManager.UpdateAsync(appUser);

            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3230, "EmailChangeTokenCreationFailureDueToAccountDeactivationFailure"), "The deactivation of the account could not be completed and thus for security reason the whole process had to " +
                    "be terminated. NewEmail={NewEmail}. Errors={Errors}", newEmail, result.Errors);
            }

            _logger.LogInformation(new EventId(2210, "EmailChangeTokenCreationSuccess"), "Successfully created email change token. " +
                    "UserId={UserId}, Email={Email}, NewEmail={NewEmail}.", appUser.Id, appUser.Email, newEmail);

            return new ReturnTokenAndCodeResponseModel(emailChangeToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4211, "EmailChangeTokenCreationFailure"), ex, "An error occurred while trying to create account email reset token. " +
                "NewEmail={NewEmail}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", newEmail, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnTokenAndCodeResponseModel> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail)
    {
        // Create an execution strategy
        var strategy = _identityDbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _identityDbContext.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                {
                    _logger.LogWarning(new EventId(3231, "UserEmailChangeFailureDueToNullUser"),
                        "Tried to change account email of null user= UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.", userId, changeEmailToken, newEmail);
                    return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
                }

                AppUser? otherUser = await _userManager.FindByEmailAsync(newEmail!);
                if (otherUser is not null)
                {
                    _logger.LogWarning(new EventId(3232, "EmailChangeFailureDueToDuplicateEmail"),
                        "Another user has the given new email and thus the process of changing the email of the account could not be completed. " +
                        "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.", userId, changeEmailToken, newEmail);
                    return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.DuplicateEmail);
                }

                var emailChangeResult = await _userManager.ChangeEmailAsync(user, newEmail, changeEmailToken);
                if (!emailChangeResult.Succeeded)
                {
                    _logger.LogWarning(new EventId(3233, "UserEmailChangeFailureDueToInvalidTokenEmailCombination"),
                        "Tried to change email of user, but the token and the new email combination seems to be invalid. " +
                        "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.", userId, changeEmailToken, newEmail);
                    return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.InvalidEmailAndEmailChangeTokenCombination);
                }

                user.UserName = newEmail;
                user.EmailConfirmed = true;
                var updateUsernameResult = await _userManager.UpdateAsync(user);
                if (!updateUsernameResult.Succeeded)
                {
                    _logger.LogWarning(new EventId(3234, "UnknownError"),
                        "Tried to change username to much email field, but something went wrong, so there needs to be a rollback. " +
                        "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.", userId, changeEmailToken, newEmail);
                    return new ReturnTokenAndCodeResponseModel(null!, LibraryReturnedCodes.UnknownError);
                }

                // Commit the transaction
                await _identityDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(new EventId(2211, "UserEmailChangeSuccess"),
                    "Successfully changed user's email account. UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.", userId, changeEmailToken, newEmail);

                //Get all the roles of the user
                List<string> userRoles = new List<string>(await _userManager.GetRolesAsync(user));
                string accessToken = await GenerateTokenAsync(user, false, userRoles);
                return new ReturnTokenAndCodeResponseModel(accessToken, LibraryReturnedCodes.NoError);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(new EventId(4212, "UserEmailChangeFailure"), ex,
                    "An error occurred while trying to change user email account. UserId= {UserId}, NewEmail= {NewEmail}. " +
                    "ExceptionMessage= {ExceptionMessage}. StackTrace= {StackTrace}.", userId, newEmail, ex.Message, ex.StackTrace);
                throw;
            }
        });
    }

    public async Task<ReturnUserAndCodeResponseModel> GetCurrentUserWithValidatedClaimsByTokenAsync(string accessToken, List<Claim> expectedClaims)
    {
        try
        {
            return await _helperMethods.StandardTokenValidationAuthenticationAndAuthorizationProcedures(accessToken, expectedClaims, new EventId(3235, "GetCurrentUserWithValidatedClaimsByTokenAsync"));
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4200, "GetCurrentUserWithValidatedClaimsByTokenAsync"), ex, "An error occurred while retrieving logged in user and validating their expected claims. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    private async Task<string> GenerateTokenAsync(AppUser user, bool isPersistent = false, List<string>? roleNames = null)
    {
        roleNames ??= new List<string>() { "User" }; //if rolenames equals null the default is user

        // Create claims for the updatedAppUser
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
        };

        List<AppRole> userRoles = new List<AppRole>();
        foreach (string roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
                userRoles.Add(role);
            }
        }

        foreach (var userRole in userRoles)
        {
            var roleClaims = await _roleManager.GetClaimsAsync(userRole);
            if (roleClaims is null)
                continue;

            claims.AddRange(roleClaims);
        }

        // Generate JWT accessToken
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = isPersistent ? DateTime.Now.AddDays(30) : DateTime.Now.AddDays(1);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //This is added later and has not been thoroughly tested
    public async Task<ReturnUserAndCodeResponseModel> CheckResetPasswordEligibilityForGivenUserId(string userId, string resetPasswordToken)
    {
        try
        {
            AppUser? user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(new EventId(9999, "CheckResetPasswordEligibilityForGivenUserIdFailureDueToNullUser"),
                    "Tried to check reset account password eligibility of null user. UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.", userId, resetPasswordToken);
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
            }

            if (!_helperMethods.IsEmailConfirmed(user, new EventId(9999, "CheckResetPasswordEligibilityForGivenUserIdFailureDueToUnconfirmedEmail"),
                "The check reset password eligibility process could not continue, because the user account is not activated. Email= {Email}"))
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);

            bool isValid = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", resetPasswordToken);
            if (!isValid)
                return new ReturnUserAndCodeResponseModel(null!, LibraryReturnedCodes.InvalidToken);


            return new ReturnUserAndCodeResponseModel(user, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CheckResetPasswordEligibilityForGivenUserIdFailure"), ex, "An error occurred while trying to check user's eligibility for account password reset. " +
                "UserId={UserId}. ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibraryReturnedCodes> UpdateAccountAsync(string accessToken, AppUser updatedAppUser)
    {
        try
        {
            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _helperMethods.StandardTokenAndUserValidationProcedures(accessToken, new EventId(9996, "UpdateAccount"));
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes != LibraryReturnedCodes.NoError)
                return returnCodeAndUserResponseModel.LibraryReturnedCodes;

            AppUser existingUser = returnCodeAndUserResponseModel.AppUser!;
            existingUser.FirstName = updatedAppUser.FirstName?.Trim() ?? existingUser.FirstName;
            existingUser.LastName = updatedAppUser.LastName?.Trim() ?? existingUser.LastName;
            existingUser.PhoneNumber = updatedAppUser.PhoneNumber?.Trim() ?? existingUser.PhoneNumber;

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(9999, "UpdateAccountFailureNoException"), "User account information could not be updated. " +
                    "UserId={UserId}. Errors={Errors}.", existingUser.Id, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(9999, "UpdateAccountSuccess"), "Successfully updated user account information. UserId={UserId}, Email={Email}.",
            existingUser.Id, updatedAppUser.Email);
            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateAccountFailure"), ex, "An error occurred while trying update the users account information. " +
                "ExceptionMessage= {ExceptionMessage}. StackTrace= {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }
}
