namespace EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationProceduresModels;
public class ReturnTokenUserIdAndCodeResponseModel
{
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public LibraryReturnedCodes LibraryReturnedCodes { get; set; }

    public ReturnTokenUserIdAndCodeResponseModel(string token, string userId, LibraryReturnedCodes libraryReturnedCodes)
    {
        Token = token;
        UserId = userId;
        LibraryReturnedCodes = libraryReturnedCodes;
    }

}
