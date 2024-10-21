namespace EshopApp.DataLibrary.Models.ResponseModels.VariantModels;

public class ReturnVariantsAndCodeResponseModel
{
    public List<Variant> Variants { get; set; } = new List<Variant>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnVariantsAndCodeResponseModel() { }
    public ReturnVariantsAndCodeResponseModel(List<Variant> variants, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var variant in variants ?? Enumerable.Empty<Variant>())
            Variants.Add(variant);
        ReturnedCode = libraryReturnedCodes;
    }
}