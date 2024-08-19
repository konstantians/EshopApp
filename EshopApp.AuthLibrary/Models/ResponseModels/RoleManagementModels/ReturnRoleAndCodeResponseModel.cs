namespace EshopApp.AuthLibrary.Models.ResponseModels.RoleManagementModels;

public class ReturnRoleAndCodeResponseModel
{
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }
    public AppRole? AppRole { get; set; }

    public ReturnRoleAndCodeResponseModel(AppRole appRole, LibraryReturnedCodes libraryReturnedCodes)
    {
        AppRole = appRole;
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
