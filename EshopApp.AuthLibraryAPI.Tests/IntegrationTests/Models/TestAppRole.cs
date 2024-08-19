using Microsoft.AspNetCore.Identity;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.Models
{
    internal class TestAppRole : IdentityRole
    {
        public List<TestCustomClaim> Claims { get; set; } = new List<TestCustomClaim>();
    }
}
