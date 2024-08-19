namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models
{
    internal class TestCustomClaim
    {
        public string? Type { get; set; }
        public string? Value { get; set; }

        public TestCustomClaim() { }

        public TestCustomClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
