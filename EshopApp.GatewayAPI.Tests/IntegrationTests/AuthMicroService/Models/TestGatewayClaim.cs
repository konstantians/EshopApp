namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.Models;
internal class TestGatewayClaim
{
    public string? Type { get; set; }
    public string? Value { get; set; }

    public TestGatewayClaim() { }

    public TestGatewayClaim(string type, string value)
    {
        Type = type;
        Value = value;
    }
}
