namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestAppAttribute
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<TestVariant> Variants { get; set; } = new List<TestVariant>();
}
