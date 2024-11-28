namespace EshopApp.DataLibrary.Models.ResponseModels.ShippingOptionModels;
public class ReturnShippingOptionsAndCodeResponseModel
{
    public List<ShippingOption> ShippingOptions { get; set; } = new List<ShippingOption>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnShippingOptionsAndCodeResponseModel() { }
    public ReturnShippingOptionsAndCodeResponseModel(List<ShippingOption> shippingOptions, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var shippingOption in shippingOptions ?? Enumerable.Empty<ShippingOption>())
            ShippingOptions.Add(shippingOption);
        ReturnedCode = libraryReturnedCodes;
    }
}
