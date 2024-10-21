namespace EshopApp.DataLibrary.Models.ResponseModels.DiscountModels;

public class ReturnDiscountsAndCodeResponseModel
{
    public List<Discount> Discounts { get; set; } = new List<Discount>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnDiscountsAndCodeResponseModel() { }
    public ReturnDiscountsAndCodeResponseModel(List<Discount> discounts, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var discount in discounts ?? Enumerable.Empty<Discount>())
            Discounts.Add(discount);
        ReturnedCode = libraryReturnedCodes;
    }
}
