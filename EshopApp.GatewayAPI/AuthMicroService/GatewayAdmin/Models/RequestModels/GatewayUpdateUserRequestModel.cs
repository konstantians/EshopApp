using System.ComponentModel.DataAnnotations;
using EshopApp.GatewayAPI.AuthMicroService.Models;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAdmin.Models.RequestModels;

public class GatewayUpdateUserRequestModel
{
    [Required]
    public GatewayAppUser? AppUser { get; set; }
    public bool ActivateEmail { get; set; }
    public string? Password { get; set; }
}
