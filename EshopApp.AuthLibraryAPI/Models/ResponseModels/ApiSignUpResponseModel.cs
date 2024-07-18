namespace EshopApp.AuthLibraryAPI.Models.ResponseModels;

public class ApiSignUpResponseModel
{   
    public string? UserId { get; set; }
    public string? ConfirmationToken { get; set; }

    public ApiSignUpResponseModel() { }

    public ApiSignUpResponseModel(string userId, string confirmationToken)
    {
        UserId = userId;
        ConfirmationToken = confirmationToken;
    }
}
