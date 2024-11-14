namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestCategory
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<TestProduct> Products { get; set; } = new List<TestProduct>();
}
