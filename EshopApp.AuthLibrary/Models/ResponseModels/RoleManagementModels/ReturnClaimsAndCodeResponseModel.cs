using System.Security.Claims;

namespace EshopApp.AuthLibrary.Models.ResponseModels.RoleManagementModels;

public class ReturnClaimsAndCodeResponseModel
{
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }
    public List<Claim> Claims { get; set; } = new List<Claim>();

    public ReturnClaimsAndCodeResponseModel(List<Claim> claims, LibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var claim in claims)
            Claims.Add(claim);
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
