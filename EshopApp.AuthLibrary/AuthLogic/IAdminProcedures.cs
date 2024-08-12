using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.AuthLogic
{
    public interface IAdminProcedures
    {
        Task<ReturnUserAndCodeResponseModel> CreateUserAccountAsync(string accessToken, List<Claim> expectedClaims, string email, string password, string? phoneNumber = null);
        Task<LibraryReturnedCodes> DeleteUserAccountAsync(string accessToken, List<Claim> expectedClaims, string userId);
        Task<ReturnUserAndCodeResponseModel?> FindUserByEmailAsync(string accessToken, List<Claim> expectedClaims, string email);
        Task<ReturnUserAndCodeResponseModel?> FindUserByIdAsync(string accessToken, List<Claim> expectedClaims, string userId);
        Task<ReturnUsersAndCodeResponseModel> GetUsersAsync(string accessToken, List<Claim> expectedClaims);
        Task<LibraryReturnedCodes> UpdateUserAccountAsync(string accessToken, List<Claim> expectedClaims, AppUser updatedUser, bool activateEmail, string? password = null);
    }
}