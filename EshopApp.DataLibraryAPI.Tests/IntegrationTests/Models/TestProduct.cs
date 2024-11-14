namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestProduct
{
    public string? Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<TestCategory> Categories { get; set; } = new List<TestCategory>();
    public List<TestVariant> Variants { get; set; } = new List<TestVariant>();
}
