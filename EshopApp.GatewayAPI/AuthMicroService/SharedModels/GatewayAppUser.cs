using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using Microsoft.AspNetCore.Identity;

namespace EshopApp.GatewayAPI.AuthMicroService.Models;

//eventually add addresses
public class GatewayAppUser : IdentityUser
{
    //TODO add cart here when the other models are added
    public List<GatewayUserCoupon> UserCoupons { get; set; } = new List<GatewayUserCoupon>();
}
