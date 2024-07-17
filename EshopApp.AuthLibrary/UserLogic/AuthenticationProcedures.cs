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

namespace EshopApp.AuthLibrary.UserLogic;


//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 200-299, for example 2200 or 2299.
internal class AuthenticationProcedures : IAuthenticationProcedures
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public AuthenticationProcedures(UserManager<AppUser> userManager, IConfiguration config, ILogger<AuthenticationProcedures> logger = null!)
    {
        _userManager = userManager;
        _logger = logger ?? NullLogger<AuthenticationProcedures>.Instance;
        _config = config;
    }

    public async Task<List<AppUser>> GetUsersAsync()
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
    }

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
            _logger.LogError(new EventId(4201, "CurrentUserCouldNotBeRetrieved"), ex, "An error occurred while retrieving logged in user. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<AppUser?> FindByUserIdAsync(string userId)
    {
        try
        {
            return await _userManager.FindByIdAsync(userId);
        }
        catch (Exception ex)
        {

            _logger.LogError(new EventId(4202, "UserRetrievalByIdFailure"), ex, "An error occurred while retrieving user with id: {UserId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<AppUser?> FindByEmailAsync(string email)
    {
        try
        {
            return await _userManager.FindByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4203, "UserRetrievalByEmailFailure"), ex, "An error occurred while retrieving user with email: {Email}. " +
                "ExceptionMessage {ExceptionMessage}. StackTrace: {StackTrace}.", email, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<(string, string)> SignUpUserAsync(AppUser appUser, string password,
        bool isPersistent)
    {
        try
        {
            var result = await _userManager.CreateAsync(appUser, password);
            if (!result.Succeeded)
                throw new ApplicationException("Failed to create user account with given credentials.");

            string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

            _logger.LogInformation(new EventId(2200, "UserRegistrationSuccess"), "Successfully created user account: UserId={UserId}, Email={Email}, Username={Username}.",
                appUser.Id, appUser.Email, appUser.UserName);

            return (appUser.Id, confirmationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4204, "UserRegistrationFailure"), ex, "An error occurred while creating user account. " +
                "Email: {Email}, Username: {Username}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> ConfirmEmailAsync(string userId, string confirmationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3200, "EmailConfirmationFailureDueToNullUser"), "Tried to confirm email of null user: " +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}.", userId, confirmationToken);
                return null!;
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);
            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3201, "EmailConfirmationFailureNoException"), "Email of user could not be confirmed: " +
                    "UserId={UserId}, ConfirmationToken={ConfirmationToken}. Errors={Errors}.", userId, confirmationToken, result.Errors);
                return null!;
            }

            string token = GenerateToken(user);

            _logger.LogInformation(new EventId(2201, "UserEmailSuccessfullyConfirmed"), "Successfully confirmed user's email account: UserId={UserId}", userId);
            return token;
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


    public async Task<(bool, string)> ChangePasswordAsync(AppUser appUser, string currentPassword, string newPassword)
    {
        try
        {
            IdentityResult result;
            if (currentPassword is not null)
            {
                result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
            }
            //this can happen if the user created an account through an external identity provider(edge case)
            else
            {
                result = await _userManager.AddPasswordAsync(appUser, newPassword);
            }

            if (!result.Succeeded && result.Errors.Where(error => error.Code == "PasswordMismatch").Count() > 0)
                return (false, "passwordMismatch");
            else if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3202, "PasswordChangeFailureNoException"), "Password could not be changed: " +
                    "UserId={UserId}, Email={Email}, Username={Username}. Errors={Errors}.",
                    appUser.Id, appUser.Email, appUser.UserName, result.Errors);
                return (false, "unknown");
            }

            _logger.LogInformation(new EventId(2202, "UserPasswordSuccessfullyChanged"), "Successfully changed user's account password. " +
                "UserId={UserId}, Email={Email}, Username={Username}.", appUser.Id, appUser.Email, appUser.UserName);
            return (true, "nothing");
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4206, "PasswordChangeFailure"), ex, "An error occurred while changing user account password. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            return (false, "unknown");
        }
    }

    public async Task<string> SignInUserAsync(string username, string password, bool isPersistent)
    {
        try
        {
            //username here is the same as email
            AppUser? user = await FindByEmailAsync(username);
            var result = await _userManager.CheckPasswordAsync(user!, password)!;

            if (!result)
            {
                _logger.LogWarning(new EventId(3203, "UserSignInFailureNoException"), "User could not be signed in, possibly, because of invalid credentials. Username={Username}.", username);
                return null!;
            }

            _logger.LogInformation(new EventId(2203, "UserSignInSuccess"), "Successfully signed in user. Username={Username}, IsPersistent={IsPersistent}.",
                username, isPersistent);
            return GenerateToken(user!, isPersistent);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4207, "UserSignInFailure"), ex, "An error occurred while trying to sign in the user. " +
                "Username: {Username}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , username, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> UpdateUserAccountAsync(AppUser appUser)
    {
        try
        {
            //here make sure that username and email are the same
            var result = await _userManager.UpdateAsync(appUser);
            if (!result.Succeeded)
                _logger.LogWarning(new EventId(3204, "UserInformationUpdateFailureNoException"), "User account information could not be updated. " +
                    "UserId={UserId}, Email={Email}, Username={Username}. " +
                    "Errors={Errors}.", appUser.Id, appUser.Email, appUser.UserName, result.Errors);
            else
                _logger.LogInformation(new EventId(2204, "UserRetrievalError"), "Successfully updated user account information. " +
                    "UserId={UserId}, Email={Email}, Username={Username}.",
                    appUser.Id, appUser.Email, appUser.UserName);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4208, "UserInformationUpdateFailure"), ex, "An error occurred while trying update the users account information. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<bool> DeleteUserAccountAsync(AppUser appUser)
    {
        try
        {
            var result = await _userManager.DeleteAsync(appUser);
            if (!result.Succeeded)
                _logger.LogWarning(new EventId(3205, "UserDeletionFailureNoException"), "User account could not be deleted. " +
                    "UserId={UserId}, Email={Email}, Username={Username}. " +
                    "Errors={Errors}.", appUser.Id, appUser.Email, appUser.UserName, result.Errors);
            else
                _logger.LogInformation(new EventId(2205, "UserDeletionSuccess"), "Successfully deleted user account. " +
                    "UserId={UserId}, Email={Email}, Username={Username}.",
                    appUser.Id, appUser.Email, appUser.UserName);

            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4209, "UserDeletionFailure"), ex, "An error occurred while trying to delete the user account. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> CreateResetPasswordTokenAsync(AppUser appUser)
    {
        try
        {
            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(appUser);
            _logger.LogInformation(new EventId(2206, "UserResetTokenCreationSuccess"), "Successfully created password reset token. " +
                    "UserId={UserId}, Email={Email}, Username={Username}.",
                    appUser.Id, appUser.Email, appUser.UserName);

            return passwordResetToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4210, "UserResetTokenCreationFailure"), ex, "An error occurred while trying to create reset account password token. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , appUser.Id, appUser.Email, appUser.UserName, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                _logger.LogWarning(new EventId(3206, "UserPasswordResetFailureDueToNullUser"), "Tried to reset account password of null user: " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.", userId, resetPasswordToken);
                return null!;
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordToken, newPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning(new EventId(3207, "UserPasswordResetFailureNoException"), "User account password could not be reset. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}. " +
                    "Errors={Errors}.", userId, resetPasswordToken, result.Errors);
                return null!;
            }

            string token = GenerateToken(user);
            _logger.LogInformation(new EventId(2207, "UserPasswordResetSuccess"), "Successfully reset account password. " +
                    "UserId={UserId}, ResetPasswordToken={ResetPasswordToken}.",
                    userId, resetPasswordToken);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4211, "UserPasswordResetFailure"), ex, "An error occurred while trying reset user's account password. " +
                "UserId: {UserId}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}."
                , userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> CreateChangeEmailTokenAsync(AppUser appUser, string newEmail)
    {
        try
        {
            string emailChangeToken = await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);
            _logger.LogInformation(new EventId(2208, "EmailChangeTokenCreationSuccess"), "Successfully created email change token. " +
                    "UserId={UserId}, Email={Email}, Username={Username}, NewEmail={NewEmail}.",
                    appUser.Id, appUser.Email, appUser.UserName, newEmail);

            return emailChangeToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4212, "EmailChangeTokenCreationFailure"), ex, "An error occurred while trying to create account email reset token. " +
                "UserId: {UserId}, Email: {Email}, Username: {Username}, NewEmail, {NewEmail}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                appUser.Id, appUser.Email, appUser.UserName, newEmail, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<string> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail)
    {

        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(new EventId(3208, "UserEmailChangeFailureDueToNullUser"), "Tried to change account email of null user: " +
                    "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.", userId, changeEmailToken, newEmail);
                return null!;
            }

            var result = await _userManager.ChangeEmailAsync(user, newEmail, changeEmailToken);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to change email.");
            }

            _logger.LogInformation(new EventId(2209, "UserEmailChangeSuccess"), "Successfully changed user's email account. " +
                "UserId={UserId}, ChangeEmailToken={ChangeEmailToken}, NewEmail={NewEmail}.",
                userId, changeEmailToken, newEmail);

            string token = GenerateToken(user);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4213, "UserEmailChangeFailure"), ex, "An error occurred while trying to change user email account. " +
                "UserId: {UserId}, NewEmail: {NewEmail}. ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                userId, newEmail, ex.Message, ex.StackTrace);
            throw;
        }
    }

    private string GenerateToken(AppUser user, bool isPersistent = false)
    {
        // Create claims for the user
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!), //here the username is equal with email
        };

        // Generate JWT token
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
