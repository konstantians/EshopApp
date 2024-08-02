using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;

namespace EshopApp.AuthLibrary.UserLogic;

public interface IAuthenticationProcedures
{
    Task<ReturnCodeAndTokenResponseModel> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail);
    Task<LibraryReturnedCodes> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword);
    Task<ReturnCodeAndTokenResponseModel> ConfirmEmailAsync(string userId, string confirmationToken);
    Task<ReturnCodeAndTokenResponseModel> CreateChangeEmailTokenAsync(string accessToken, string newEmail);
    Task<ReturnCodeAndTokenResponseModel> CreateResetPasswordTokenAsync(string email);
    Task<LibraryReturnedCodes> DeleteAccountAsync(string userId, string accessToken);
    Task<AppUser?> FindByEmailAsync(string email);
    Task<AppUser?> FindByUserIdAsync(string userId);
    Task<AppUser?> GetCurrentUserByToken(string token);
    Task<LibSignUpResponseModel> SignUpAsync(string email, string phoneNumber, string password);
    Task<ReturnCodeAndTokenResponseModel> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword);
    Task<ReturnCodeAndTokenResponseModel> SignInAsync(string username, string password, bool isPersistent);
    Task<bool> UpdateAccountAsync(AppUser appUser);
}