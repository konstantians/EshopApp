namespace EshopApp.DataLibrary.Models.ResponseModels.VariantModels;

public class ReturnVariantImageAndCodeResponseModel
{
    public VariantImage? VariantImage { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnVariantImageAndCodeResponseModel() { }
    public ReturnVariantImageAndCodeResponseModel(VariantImage variantImage, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        VariantImage = variantImage;
        ReturnedCode = libraryReturnedCodes;
    }
}
