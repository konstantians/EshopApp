namespace EshopApp.DataLibrary.Models.ResponseModels.DiscountModels;

public class ReturnDiscountAndCodeResponseModel
{
    public Discount? Discount { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnDiscountAndCodeResponseModel() { }
    public ReturnDiscountAndCodeResponseModel(Discount discount, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Discount = discount;
        ReturnedCode = libraryReturnedCodes;
    }

}
