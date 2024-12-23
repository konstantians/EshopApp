namespace EshopApp.AuthLibraryAPI.Models.ResponseModels.AuthenticationModels;

public class ApiForgotPasswordResponseModel
{
    public string? Token { get; set; }
    public string? UserId { get; set; }

    public ApiForgotPasswordResponseModel(string token, string userId)
    {
        Token = token;
        UserId = userId;
    }
}
