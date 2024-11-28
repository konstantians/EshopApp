namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.Models.RequestModels.DiscountModels;
internal class TestCreateDiscountRequestModel
{
    public string? Name { get; set; }
    public int? Percentage { get; set; }
    public bool? IsDeactivated { get; set; }
}
