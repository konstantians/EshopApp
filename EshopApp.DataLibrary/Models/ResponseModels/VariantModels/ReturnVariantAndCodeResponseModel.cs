namespace EshopApp.DataLibrary.Models.ResponseModels.VariantModels;

public class ReturnVariantAndCodeResponseModel
{
    public Variant? Variant { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnVariantAndCodeResponseModel() { }
    public ReturnVariantAndCodeResponseModel(Variant variant, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Variant = variant;
        ReturnedCode = libraryReturnedCodes;
    }
}
