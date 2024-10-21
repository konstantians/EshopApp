namespace EshopApp.DataLibrary.Models.ResponseModels.VariantModels;

public class ReturnVariantsImagesAndCodeResponseModel
{
    public List<VariantImage> VariantImages { get; set; } = new List<VariantImage>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnVariantsImagesAndCodeResponseModel() { }
    public ReturnVariantsImagesAndCodeResponseModel(List<VariantImage> variantImages, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var variant in variantImages ?? Enumerable.Empty<VariantImage>())
            VariantImages.Add(variant);
        ReturnedCode = libraryReturnedCodes;
    }

}
