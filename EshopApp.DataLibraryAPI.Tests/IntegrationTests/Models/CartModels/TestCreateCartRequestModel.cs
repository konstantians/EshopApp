namespace EshopApp.DataLibraryAPI.Tests.IntegrationTests.Models.CartModels;
internal class TestCreateCartRequestModel
{
    public string? UserId { get; set; }
    public List<TestCreateCartItemRequestModel> CreateCartItemRequestModels { get; set; } = new List<TestCreateCartItemRequestModel>();
}
