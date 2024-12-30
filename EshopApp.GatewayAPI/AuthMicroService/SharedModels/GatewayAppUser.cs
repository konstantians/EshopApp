using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using Microsoft.AspNetCore.Identity;

namespace EshopApp.GatewayAPI.AuthMicroService.Models;

//eventually add addresses
public class GatewayAppUser : IdentityUser
{
    public GatewayCart? Cart { get; set; }
    public List<GatewayUserCoupon> UserCoupons { get; set; } = new List<GatewayUserCoupon>();
    public List<GatewayOrder> Orders { get; set; } = new List<GatewayOrder>();
}
