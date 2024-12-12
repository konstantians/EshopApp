namespace EshopApp.GatewayAPI.Models.ServiceRequestModels;

public class GatewayApiSignInServiceRequestModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
