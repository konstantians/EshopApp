namespace EshopApp.AuthLibraryAPI.Models.RequestModels;

public class ApiSignUpRequestModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
}
