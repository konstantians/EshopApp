namespace EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationModels;

public class ReturnTokenAndCodeResponseModel
{
    public string? Token { get; set; }
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }

    public ReturnTokenAndCodeResponseModel(string token, LibraryReturnedCodes libraryReturnedCodes)
    {
        Token = token;
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
