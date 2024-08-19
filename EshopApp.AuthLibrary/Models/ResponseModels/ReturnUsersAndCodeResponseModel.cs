namespace EshopApp.AuthLibrary.Models.ResponseModels;

public class ReturnUsersAndCodeResponseModel
{
    public List<AppUser> AppUsers { get; set; } = new List<AppUser>();
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }

    public ReturnUsersAndCodeResponseModel() { }
    public ReturnUsersAndCodeResponseModel(List<AppUser> appUsers, LibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (AppUser appUser in appUsers ?? Enumerable.Empty<AppUser>())
            AppUsers.Add(appUser);
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
