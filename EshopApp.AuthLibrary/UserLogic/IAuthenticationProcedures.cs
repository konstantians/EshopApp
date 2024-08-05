using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using Microsoft.AspNetCore.Authentication;

namespace EshopApp.AuthLibrary.UserLogic;

public interface IAuthenticationProcedures
{
    Task<ReturnCodeAndTokenResponseModel> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail);
    Task<LibraryReturnedCodes> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword);
    Task<ReturnCodeAndTokenResponseModel> ConfirmEmailAsync(string userId, string confirmationToken);
    Task<ReturnCodeAndTokenResponseModel> CreateChangeEmailTokenAsync(string accessToken, string newEmail);
    Task<ReturnCodeAndTokenResponseModel> CreateResetPasswordTokenAsync(string email);
    Task<LibraryReturnedCodes> DeleteAccountAsync(string accessToken);
    Task<AppUser?> GetCurrentUserByToken(string token);
    Task<LibSignUpResponseModel> SignUpAsync(string email, string phoneNumber, string password);
    Task<ReturnCodeAndTokenResponseModel> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword);
    Task<ReturnCodeAndTokenResponseModel> SignInAsync(string username, string password, bool isPersistent);
    Task<bool> UpdateAccountAsync(AppUser appUser);
    AuthenticationProperties GetExternalIdentityProvidersProperties(string identityProviderName, string redirectUrl);
    Task<ReturnCodeAndTokenResponseModel> HandleExternalSignInCallbackAsync();
    Task<IEnumerable<AuthenticationScheme>> GetExternalIdentityProvidersAsync();
}