namespace EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationModels;

public class LibSignUpResponseModel : ReturnTokenAndCodeResponseModel
{
    public string? UserId { get; set; }

    public LibSignUpResponseModel(string confirmationEmailToken, string userId, LibraryReturnedCodes libraryReturnedCodes) : base(confirmationEmailToken, libraryReturnedCodes)
    {
        UserId = userId;
    }
}
