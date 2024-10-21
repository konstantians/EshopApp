namespace EshopApp.DataLibrary.Models.ResponseModels.ProductModels;

public class ReturnProductAndCodeResponseModel
{
    public Product? Product { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnProductAndCodeResponseModel() { }
    public ReturnProductAndCodeResponseModel(Product product, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Product = product;
        ReturnedCode = libraryReturnedCodes;
    }
}
