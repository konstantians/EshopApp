namespace EshopApp.DataLibrary.Models.ResponseModels.ProductModels;

public class ReturnProductsAndCodeResponseModel
{
    public List<Product> Products { get; set; } = new List<Product>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnProductsAndCodeResponseModel() { }
    public ReturnProductsAndCodeResponseModel(List<Product> products, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var product in products ?? Enumerable.Empty<Product>())
            Products.Add(product);
        ReturnedCode = libraryReturnedCodes;
    }
}
