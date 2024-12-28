using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
using Microsoft.AspNetCore.Identity;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.SharedModels;
internal class TestGatewayAppUser : IdentityUser
{
    public TestGatewayCart? Cart { get; set; }
    public List<TestGatewayUserCoupon> UserCoupons { get; set; } = new List<TestGatewayUserCoupon>();
}
