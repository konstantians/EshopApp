namespace EshopApp.GatewayAPI.Models.RequestModels;

public class GatewayApiResetPasswordRequestModel
{
    public string? UserId { get; set; }
    public string? Token { get; set; }
    public string? Password { get; set; }
}
