using EshopApp.GatewayAPI.AuthMicroService.SharedModels;
using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using Microsoft.AspNetCore.Identity;

namespace EshopApp.GatewayAPI.AuthMicroService.Models;

//eventually add addresses
public class GatewayAppUser : IdentityUser
{
    public GatewayAddress? Address { get; set; }
    public GatewayCart? Cart { get; set; }
    public List<GatewayUserCoupon> UserCoupons { get; set; } = new List<GatewayUserCoupon>();
    public List<GatewayOrder> Orders { get; set; } = new List<GatewayOrder>();
    public string? UserRoleName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool HasPassword { get; set; }
}
