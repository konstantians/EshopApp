using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using EshopApp.AuthLibrary.Models;
using Microsoft.AspNetCore.Authentication;
using EshopApp.AuthLibrary.Models.ResponseModels;

namespace EshopApp.AuthLibrary.UserLogic;


//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 200-299, for example 2200 or 2299.
public class AuthenticationProcedures : IAuthenticationProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager; 
    private readonly ILogger<AuthenticationProcedures> _logger;
    private readonly IConfiguration _config;
    private readonly AppIdentityDbContext _identityDbContext;

    public AuthenticationProcedures(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppIdentityDbContext appIdentityDbContext,
        IConfiguration config, ILogger<AuthenticationProcedures> logger = null!)
    {
        _identityDbContext = appIdentityDbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
        _config = config;
    }

    //add this maybe to the the admin procedures
    /*public async Task<List<AppUser>> GetUsersAsync()
    {
        try
        {
            return await _userManager.Users.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4200, "UsersRetrievalFailure"), ex, "An error occurred while retrieving users. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }*/

    public async Task<AppUser?> GetCurrentUserByToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            return await _userManager.FindByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4200, "CurrentUserCouldNotBeRetrieved"), ex, "An error occurred while retrieving logged in user. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    private async Task<AppUser?> FindByUserIdAsync(string userId)
    {
        try
        {
            return await _userManager.FindByIdAsync(userId);
        }
        catch (Exception ex)
        {

            _logger.LogError(new EventId(4201, "UserRetrievalByIdFailure"), ex, "An error occurred while retrieving user with id: {UserId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    private async Task<AppUser?> FindByEmailAsync(string email)
    {
        try
        {
            return await _userManager.FindByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4202, "UserRetrievalByEmailFailure"), ex, "An error occurred while retrieving user with email: {Email}. " +
                "ExceptionMessage {ExceptionMessage}. StackTrace: {StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibSignUpResponseModel> SignUpAsync(string email, string phoneNumber, string password)
    {
        try
        {
            AppUser? appUser = await FindByEmailAsync(email);
            if (appUser is not null)
            {
                _logger.LogWarning(new EventId(3200, "SignUpFailureDueToDuplicateEmail"), "Another user has the given email and thus the signup process can not proceed. Email={Email}.", email);
                return new LibSignUpResponseModel(null!, null!, LibraryReturnedCodes.DuplicateEmail);
            }

            appUser = new AppUser();
            appUser.Id = Guid.NewGuid().ToString();

            //make sure that the guid is unique(extreme edge case)
            AppUser? otherUser = await FindByUserIdAsync(appUser.Id);
            while (otherUser is not null)
                appUser.Id = Guid.NewGuid().ToString();  

            appUser.UserName = email;
            appUser.Email = email;
            appUser.PhoneNumber = phoneNumber;

            var result = await _userManager.CreateAsync(appUser, password);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3201, "UserRegistrationFailureUnkownError"), "An error occurred while creating user account, but an exception was not thrown. Email: {Email}, Errors: {Errors}.", 
                    appUser.Email, result.Errors);
                return new LibSignUpResponseModel(null!, null!, LibraryReturnedCodes.UnknownError);
            }

            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            _logger.LogInformation(new EventId(2200, "UserRegistrationSuccess"), "Successfully created user account: UserId={UserId}, Email={Email}, Username={Username}.",
                appUser.Id, appUser.Email, appUser.UserName);

            return new LibSignUpResponseModel(confirmationToken, appUser.Id, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4204, "UserRegistrationFailure"), ex, "An error occurred while creating user account. " +
                "Email: {Email}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCodeAndTokenResponseModel> ConfirmEmailAsync(string userId, string confirmationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3202, "EmailConfirmationFailureDueToNullUser"), "Tried to confirm email of null user: " +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}.", userId, confirmationToken);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3203, "EmailConfirmationFailureNoException"), "Email of user could not be confirmed: " +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}. Errors={Errors}.", userId, confirmationToken, result.Errors);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UnknownError);
            }

            string accessToken = GenerateToken(user);

            _logger.LogInformation(new EventId(2201, "UserEmailSuccessfullyConfirmed"), "Successfully confirmed user's email account: UserId={UserId}", userId);
            return new ReturnCodeAndTokenResponseModel(accessToken, LibraryReturnedCodes.NoError);

        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4205, "EmailConfirmationFailure"), ex, "An error occurred while confirming user email account. " +
                "UserId: {UserId}, ConfirmationToken: {ConfirmationToken}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , userId, confirmationToken, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibraryReturnedCodes> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword)
    {
        try
        {
            //this is checked by the api accessToken validation. These error happening is an extreme edge case 
            AppUser? appUser = await GetCurrentUserByToken(accessToken);
            if (appUser is null)
            {
                _logger.LogWarning(new EventId(3204, "EmailChangeTokenCreationFailureDueToValidTokenButUserNotInSystem"), "The token was valid, but it does not correspond to any user in the system and thus the process of changing" +
                    " password could not proceed. AccessToken={AccessToken}", accessToken);
                return LibraryReturnedCodes.ValidTokenButUserNotInSystem;
            }

            IdentityResult result;
            if (currentPassword is not null)
                result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
            //this can happen if the appUser created an account through an external identity provider(edge case)
            else
                result = await _userManager.AddPasswordAsync(appUser, newPassword);

            if (!result.Succeeded && result.Errors.Where(error => error.Code == "PasswordMismatch").Count() > 0)
                return LibraryReturnedCodes.PasswordMissmatch;
            else if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3205, "PasswordChangeFailureNoException"), "Password could not be changed: " +
                    "UserId={UserId}, Email={Email}, Username={Username}. Errors={Errors}.", appUser.Id, appUser.Email, appUser.UserName, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(2202, "UserPasswordSuccessfullyChanged"), "Successfully changed user's account password. " +
                "UserId={UserId}, Email={Email}, Username={Username}.", appUser.Id, appUser.Email, appUser.UserName);
            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4206, "PasswordChangeFailure"), ex, "An error occurred while changing user account password. " +
                "AccessToken: {AccesToken}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", accessToken, ex.Message, ex.StackTrace);
            return LibraryReturnedCodes.UnknownError;
        }
    }

    public async Task<ReturnCodeAndTokenResponseModel> SignInAsync(string email, string password, bool isPersistent)
    {
        try
        {
            AppUser? user = await FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3206, "SignInFailureDueToNullUser"), "Tried to sign in null user: Email={Email}.", email);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenEmail);
            }

            bool isLockedOut = await _userManager.IsLockedOutAsync(user);
            if (isLockedOut)
            {
                _logger.LogWarning(new EventId(3207, "SignInFailureDueToAccountLock"), "User with locked account tried to sign in: Email={Email}.", email);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserAccountLocked);
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning(new EventId(3208, "SignInFailureDueToUnconfirmedEmail"), "User with unconfirmed email tried to sign in: Email={Email}.", email);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);
            }

            var result = await _userManager.CheckPasswordAsync(user!, password)!;
            if (!result)
            {
                _logger.LogWarning(new EventId(3209, "UserSignInFailureDueToInvalidCredentials"), "User could not be signed in, because of invalid credentials. Email={Email}.", email);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.InvalidCredentials);
            }

            _logger.LogInformation(new EventId(2203, "UserSignInSuccess"), "Successfully signed in user. Username={Username}, IsPersistent={IsPersistent}.", email, isPersistent);
            string accessToken = GenerateToken(user!, isPersistent);

            return new ReturnCodeAndTokenResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4207, "UserSignInFailure"), ex, "An error occurred while trying to sign in the user. " +
                "Username: {Username}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , email, ex.Message, ex.StackTrace);
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
            _logger.LogError(new EventId(4208, "ExternalIdentityProvidersRetrievalFailure"), ex, "An error occurred while trying to get the external identity providers. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
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
            _logger.LogError(new EventId(4209, "ExternalIdentityProviderPropertiesRetrievalFailure"), ex, "An error occurred while trying to get the external identity provider's properties. " +
                "IdentityProviderName: {IdentityProviderName}, RedirectUrl: {RedirectUrl}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
            identityProviderName, redirectUrl, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCodeAndTokenResponseModel> HandleExternalSignInCallbackAsync()
    {
        try
        {
            ExternalLoginInfo? loginInfo = await _signInManager.GetExternalLoginInfoAsync();
            string? accessToken = null;
            if (loginInfo is null)
            {
                _logger.LogWarning(new EventId(3210, "ExternalSignInFailureDueToNullLoginInfo"), "login info of external identity provider was not received.");
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.LoginInfoNotReceivedFromIdentityProvider);
            }

            string email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email)!;
            string username = email;
            //if the returned information does not contain email give up
            if (email is null)
            {
                _logger.LogWarning(new EventId(3211, "ExternalSignInFailureBecauseEmailClaimIsMissing"), "Sign in faulre, because the email claim of the user was not received from the external identity provider.");
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.EmailClaimNotReceivedFromIdentityProvider);
            }

            AppUser? user = await FindByEmailAsync(email);
            //initially try to see if a local account and an external login already exist by trying to loging 
            var result = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false, bypassTwoFactor: false);
            if (result.Succeeded)
            {
                _logger.LogInformation(new EventId(2204, "SuccessfulExternalUserSignIn"), "Successfully signed in user with external login provider.");
                accessToken = GenerateToken(user!, false);
                return new ReturnCodeAndTokenResponseModel(accessToken!, LibraryReturnedCodes.NoError);
            }

            //if the appUser has already a local account and not an external login, try to connect the incoming external login with it
            if (user is not null)
            {
                //activating the email here for edge case where a appUser tries to first create a local account, does not activate it and then presses continue with google
                user.EmailConfirmed = true;

                await _userManager.AddLoginAsync(user, loginInfo);
                _logger.LogInformation(new EventId(2205, "SuccessfulExternalLoginAndLocalAccountLink"), "Successfully linked external login to user local account and signed them in. Email={Email}.", email);
                accessToken = GenerateToken(user!, false);
                return new ReturnCodeAndTokenResponseModel(accessToken, LibraryReturnedCodes.NoError);
            }

            //if the appUser does not have a local account and also not an external login create the local account and connect it with the incoming external login
            //make sure that the guid is unique(extreme edge case)
            string guid = Guid.NewGuid().ToString();
            AppUser? otherUser = await FindByUserIdAsync(guid);
            while (otherUser is not null)
                guid = Guid.NewGuid().ToString();

            user = new AppUser() { Id = guid, UserName = username, Email = email, EmailConfirmed = true };
            //TODO maybe add a transaction here, because an exception can happen between createasync and addloginAsync, but it is not so dangerous, because next time the login will be added without any issues, so think about it.
            await _userManager.CreateAsync(user);
            await _userManager.AddLoginAsync(user, loginInfo);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation(new EventId(2206, "SuccessfulLoginAndCreationOfLocalAccountWithExternalLogin"), "Successfully created a local account linked the external login to it and signed the user in. Email={Email}.", email);

            accessToken = GenerateToken(user!, false);
            return new ReturnCodeAndTokenResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4210, "ExternalUserSignFailure"), ex, "An error occurred while trying to sign in the user with external identity provider. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<LibraryReturnedCodes> DeleteAccountAsync(string accessToken)
    {
        try
        {
            AppUser? appUser = await GetCurrentUserByToken(accessToken);
            if (appUser is null)
            {
                _logger.LogWarning(new EventId(3212, "DeleteAccountFailureDueToValidTokenButUserNotInSystem"), "The token was valid, but it does not correspond to any user in the system and thus the process of deleting account. " +
                    "AccessToken={AccessToken}.", accessToken);
                return LibraryReturnedCodes.ValidTokenButUserNotInSystem;
            }

            var result = await _userManager.DeleteAsync(appUser);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3213, "UserDeletionFailureNoException"), "User account could not be deleted, but no exception was thrown. " +
                "AccessToken={AccessToken}, Email={Email}. Errors={Errors}.", accessToken, appUser.Email, result.Errors);
                return LibraryReturnedCodes.UnknownError;
            }

            _logger.LogInformation(new EventId(2207, "UserDeletionSuccess"), "Successfully deleted user account. AccessToken={AccessToken}, Email={Email}", accessToken, appUser.Email);

            return LibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4211, "UserDeletionFailure"), ex, "An error occurred while trying to delete the user account. " +
                "AccessToken={AccessToken}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", accessToken, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCodeAndTokenResponseModel> CreateResetPasswordTokenAsync(string email)
    {
        try
        {
            AppUser? appUser = await FindByEmailAsync(email!);

            if (appUser is null)
            {
                _logger.LogWarning(new EventId(3214, "UserResetTokenCreationFailureDueToNullUser"), "Tried to create reset password token for null user: Email={Email}.", email);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenEmail);
            }

            if (!appUser.EmailConfirmed)
            {
                _logger.LogWarning(new EventId(3215, "UserResetTokenFailureDueToUnconfirmedEmail"), "User with unconfirmed email tried to create reset token password: Email={Email}.", email);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            _logger.LogInformation(new EventId(2208, "UserResetTokenCreationSuccess"), "Successfully created password reset token. " +
                    "UserId={UserId}, Email={Email}, Username={Username}.",
                    appUser.Id, appUser.Email, appUser.UserName);

            return new ReturnCodeAndTokenResponseModel(passwordResetToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4212, "UserResetTokenCreationFailure"), ex, "An error occurred while trying to create reset account password token. " +
                "Email: {Email}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCodeAndTokenResponseModel> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3216, "UserPasswordResetFailureDueToNullUser"), "Tried to reset account password of null user: " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.", userId, resetPasswordToken);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning(new EventId(3217, "ResetPasswordFailureDueToUnconfirmedEmail"), "User with unconfirmed email tried to reset their password: UserId={UserId}.", userId);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserAccountNotActivated);
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3218, "UserPasswordResetFailureNoException"), "User account password could not be reset. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}. " +
                    "Errors={Errors}.", userId, resetPasswordToken, result.Errors);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UnknownError);
            }

            string accessToken = GenerateToken(user);
            _logger.LogInformation(new EventId(2209, "UserPasswordResetSuccess"), "Successfully reset account password. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.",
                    userId, resetPasswordToken);

            return new ReturnCodeAndTokenResponseModel(accessToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4213, "UserPasswordResetFailure"), ex, "An error occurred while trying reset user's account password. " +
                "UserId: {UserId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCodeAndTokenResponseModel> CreateChangeEmailTokenAsync(string accessToken, string newEmail)
    {
        try
        {
            AppUser? appUser = await GetCurrentUserByToken(accessToken);
            if (appUser is null)
            { 
                _logger.LogWarning(new EventId(3219, "EmailChangeTokenCreationFailureDueToValidTokenButUserNotInSystem"), "The token was valid, but it does not correspond to any user in the system and thus the process of creating " +
                    "the email change token could not be completed. Token={Token}, NewEmail={NewEmail}.", accessToken, newEmail);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.ValidTokenButUserNotInSystem);
            }

            AppUser? otherUser = await FindByEmailAsync(newEmail!);
            if (otherUser is not null)
            {
                _logger.LogWarning(new EventId(3220, "EmailChangeTokenCreationFailureDueToDuplicateEmail"), "Another user has the given new email and thus the process of creating email change token could not be completed. " +
                    "Token={Token}, NewEmail={NewEmail}.", accessToken, newEmail);
                return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.DuplicateEmail);
            }

            string emailChangeToken = await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);
            _logger.LogInformation(new EventId(2210, "EmailChangeTokenCreationSuccess"), "Successfully created email change token. " +
                    "UserId={UserId}, Email={Email}, NewEmail={NewEmail}.",
                    appUser.Id, appUser.Email, newEmail);

            return new ReturnCodeAndTokenResponseModel(emailChangeToken, LibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4214, "EmailChangeTokenCreationFailure"), ex, "An error occurred while trying to create account email reset token. " +
                "Token:{token}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", accessToken, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCodeAndTokenResponseModel> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail)
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
                    _logger.LogWarning(new EventId(3221, "UserEmailChangeFailureDueToNullUser"),
                        "Tried to change account email of null user: UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.",
                        userId, changeEmailToken, newEmail);
                    return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UserNotFoundWithGivenId);
                }

                AppUser? otherUser = await FindByEmailAsync(newEmail!);
                if (otherUser is not null)
                {
                    _logger.LogWarning(new EventId(3222, "EmailChangeFailureDueToDuplicateEmail"),
                        "Another user has the given new email and thus the process of changing the email of the account could not be completed. " +
                        "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.",
                        userId, changeEmailToken, newEmail);
                    return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.DuplicateEmail);
                }

                var emailChangeResult = await _userManager.ChangeEmailAsync(user, newEmail, changeEmailToken);
                if (!emailChangeResult.Succeeded)
                {
                    _logger.LogWarning(new EventId(3223, "UserEmailChangeFailureDueToInvalidTokenEmailCombination"),
                        "Tried to change email of user, but the token and the new email combination seems to be invalid: " +
                        "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.",
                        userId, changeEmailToken, newEmail);
                    return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.InvalidEmailAndEmailChangeTokenCombination);
                }

                user.UserName = newEmail;
                var updateUsernameResult = await _userManager.UpdateAsync(user);
                if (!updateUsernameResult.Succeeded)
                {
                    _logger.LogWarning(new EventId(3224, "UnknownError"),
                        "Tried to change username to much email field, but something went wrong, so there needs to be a rollback: " +
                        "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.",
                        userId, changeEmailToken, newEmail);
                    return new ReturnCodeAndTokenResponseModel(null!, LibraryReturnedCodes.UnknownError);
                }

                // Commit the transaction
                await _identityDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(new EventId(2211, "UserEmailChangeSuccess"),
                    "Successfully changed user's email account. UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.",
                    userId, changeEmailToken, newEmail);

                string accessToken = GenerateToken(user);
                return new ReturnCodeAndTokenResponseModel(accessToken, LibraryReturnedCodes.NoError);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(new EventId(4215, "UserEmailChangeFailure"), ex,
                    "An error occurred while trying to change user email account. UserId: {UserId}, NewEmail: {NewEmail}. " +
                    "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                    userId, newEmail, ex.Message, ex.StackTrace);
                throw;
            }
        });
    }

    //TODO Think about how to do this...
    public async Task<bool> UpdateAccountAsync(AppUser appUser)
    {
        try
        {
            //here make sure that email and email are the same
            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
                _logger.LogWarning(new EventId(3225, "UserInformationUpdateFailureNoException"), "User account information could not be updated. " +
                    "UserId={UserId}, Email={Email}. Errors={Errors}.", appUser.Id, appUser.Email, result.Errors);
            else
                _logger.LogInformation(new EventId(2212, "UserRetrievalError"), "Successfully updated user account information. UserId={UserId}, Email={Email}.",
                    appUser.Id, appUser.Email);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4216, "UserInformationUpdateFailure"), ex, "An error occurred while trying update the users account information. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    private string GenerateToken(AppUser user, bool isPersistent = false)
    {
        // Create claims for the appUser
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!), //here the email is equal with email
        };

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
}
