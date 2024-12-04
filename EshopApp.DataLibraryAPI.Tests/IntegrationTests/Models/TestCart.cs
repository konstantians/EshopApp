namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models;
internal class TestCart
{
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TestCartItem> CartItems { get; set; } = new List<TestCartItem>();
}
