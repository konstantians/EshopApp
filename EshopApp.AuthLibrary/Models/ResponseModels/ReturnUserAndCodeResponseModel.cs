namespace EshopApp.AuthLibrary.Models.ResponseModels;

public class ReturnUserAndCodeResponseModel
{
    public AppUser? AppUser { get; set; }
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }

    public ReturnUserAndCodeResponseModel()
    {

    }

    public ReturnUserAndCodeResponseModel(AppUser appUser, LibraryReturnedCodes libraryReturnedCodes)
    {
        AppUser = appUser;
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
