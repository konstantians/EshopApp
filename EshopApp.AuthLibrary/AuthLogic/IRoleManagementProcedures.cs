using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibrary.Models.ResponseModels.RoleManagementModels;
using EshopApp.AuthLibraryAPI.Models;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.AuthLogic
{
    public interface IRoleManagementProcedures
    {
        Task<LibraryReturnedCodes> AddRoleToUserAsync(string accessToken, List<Claim> expectedClaims, string userId, string roleId);
        Task<ReturnRoleAndCodeResponseModel> CreateRoleAsync(string accessToken, List<Claim> expectedClaims, string roleName, List<CustomClaim> claims);
        Task<LibraryReturnedCodes> DeleteRoleAsync(string accessToken, List<Claim> expectedClaims, string roleId);
        Task<ReturnClaimsAndCodeResponseModel> GetAllUniqueClaimsInSystemAsync(string accessToken, List<Claim> expectedClaims);
        Task<ReturnRoleAndCodeResponseModel> GetRoleByIdAsync(string accessToken, List<Claim> expectedClaims, string roleId);
        Task<ReturnRoleAndCodeResponseModel> GetRoleByNameAsync(string accessToken, List<Claim> expectedClaims, string roleName);
        Task<ReturnRolesAndCodeResponseModel> GetRolesAsync(string accessToken, List<Claim> expectedClaims);
        Task<ReturnRolesAndCodeResponseModel> GetRolesOfUserAsync(string accessToken, List<Claim> expectedClaims, string userId);
        Task<ReturnUsersAndCodeResponseModel> GetUsersOfGivenRoleAsync(string accessToken, List<Claim> expectedClaims, string roleId);
        Task<LibraryReturnedCodes> RemoveRoleFromUserAsync(string accessToken, List<Claim> expectedClaims, string userId, string roleId);
        Task<LibraryReturnedCodes> ReplaceRoleOfUserAsync(string accessToken, List<Claim> expectedClaims, string userId, string currentRoleId, string newRoleId);
        Task<ReturnRoleAndCodeResponseModel> UpdateClaimsOfRoleAsync(string accessToken, List<Claim> expectedClaims, string roleId, List<CustomClaim> updatedClaims);
    }
}