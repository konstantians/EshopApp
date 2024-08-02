namespace EshopApp.AuthLibrary.Models.ResponseModels;

public class ReturnCodeAndTokenResponseModel
{
    public string? Token { get; set; }
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }

    public ReturnCodeAndTokenResponseModel(string token, LibraryReturnedCodes libraryReturnedCodes)
    {
        Token = token;
        LibraryReturnedCodes = libraryReturnedCodes;
    }
}
