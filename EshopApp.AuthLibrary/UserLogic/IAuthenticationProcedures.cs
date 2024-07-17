using EshopApp.AuthLibrary.Models;
using Microsoft.AspNetCore.Identity;

namespace EshopApp.AuthLibrary.UserLogic
{
    internal interface IAuthenticationProcedures
    {
        Task<string> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail);
        Task<(bool, string)> ChangePasswordAsync(AppUser appUser, string currentPassword, string newPassword);
        Task<string> ConfirmEmailAsync(string userId, string confirmationToken);
        Task<string> CreateChangeEmailTokenAsync(AppUser appUser, string newEmail);
        Task<string> CreateResetPasswordTokenAsync(AppUser appUser);
        Task<bool> DeleteUserAccountAsync(AppUser appUser);
        Task<AppUser?> FindByEmailAsync(string email);
        Task<AppUser?> FindByUserIdAsync(string userId);
        Task<AppUser?> GetCurrentUserByToken(string token);
        Task<List<AppUser>> GetUsersAsync();
        Task<(string, string)> SignUpUserAsync(AppUser appUser, string password, bool isPersistent);
        Task<string> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword);
        Task<string> SignInUserAsync(string username, string password, bool isPersistent);
        Task<bool> UpdateUserAccountAsync(AppUser appUser);
    }
}