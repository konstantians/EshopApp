using Microsoft.AspNetCore.Identity;

namespace EshopApp.AuthLibrary.Models.ResponseModels.RoleManagementModels;

public class ReturnRolesAndCodeResponseModel
{
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }
    public List<AppRole> AppRoles { get; set; } = new List<AppRole>();

    public ReturnRolesAndCodeResponseModel(List<AppRole> appRoles, LibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var role in appRoles)
            AppRoles.Add(role);
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
