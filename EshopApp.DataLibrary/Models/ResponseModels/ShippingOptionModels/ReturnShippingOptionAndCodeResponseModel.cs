namespace EshopApp.DataLibrary.Models.ResponseModels.ShippingOptionModels;
public class ReturnShippingOptionAndCodeResponseModel
{
    public ShippingOption? ShippingOption { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnShippingOptionAndCodeResponseModel() { }
    public ReturnShippingOptionAndCodeResponseModel(ShippingOption shippingOption, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        ShippingOption = shippingOption;
        ReturnedCode = libraryReturnedCodes;
    }
}
