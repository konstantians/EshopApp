namespace EshopApp.AuthLibrary.Models.ResponseModels;

public class LibSignUpResponseModel : ReturnCodeAndTokenResponseModel
{
    public string? UserId { get; set; }

    public LibSignUpResponseModel(string confirmationEmailToken, string userId, LibraryReturnedCodes libraryReturnedCodes) : base(confirmationEmailToken, libraryReturnedCodes)
    {
        UserId = userId;
    }
}
