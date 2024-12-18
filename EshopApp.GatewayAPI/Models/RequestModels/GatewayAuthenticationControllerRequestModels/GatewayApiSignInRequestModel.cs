namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayAuthenticationControllerRequestModels;

public class GatewayApiSignInRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
