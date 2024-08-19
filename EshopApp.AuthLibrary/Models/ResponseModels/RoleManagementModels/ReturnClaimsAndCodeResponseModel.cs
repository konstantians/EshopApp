using EshopApp.AuthLibraryAPI.Models;

namespace EshopApp.AuthLibrary.Models.ResponseModels.RoleManagementModels;

public class ReturnClaimsAndCodeResponseModel
{
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }
    public List<CustomClaim> Claims { get; set; } = new List<CustomClaim>();

    public ReturnClaimsAndCodeResponseModel(List<CustomClaim> claims, LibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var claim in claims ?? Enumerable.Empty<CustomClaim>())
            Claims.Add(claim);
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
