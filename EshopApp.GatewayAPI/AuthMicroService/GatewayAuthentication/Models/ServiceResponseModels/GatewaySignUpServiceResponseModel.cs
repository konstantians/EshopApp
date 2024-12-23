namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.ServiceResponseModels;

public class GatewaySignUpServiceResponseModel
{
    public string? UserId { get; set; }
    public string? ConfirmationToken { get; set; }
}
