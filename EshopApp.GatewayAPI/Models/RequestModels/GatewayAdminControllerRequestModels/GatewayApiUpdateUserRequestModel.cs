using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayAdminControllerRequestModels;

public class GatewayApiUpdateUserRequestModel
{
    [Required]
    public GatewayAppUser? AppUser { get; set; }
    public bool ActivateEmail { get; set; }
    public string? Password { get; set; }
}
