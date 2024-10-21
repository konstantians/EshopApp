namespace EshopApp.DataLibrary.Models.ResponseModels.AttributeModels;

public class ReturnAttributesAndCodeResponseModel
{
    public List<AppAttribute> Attributes { get; set; } = new List<AppAttribute>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnAttributesAndCodeResponseModel() { }
    public ReturnAttributesAndCodeResponseModel(List<AppAttribute> attributes, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var attribute in attributes ?? Enumerable.Empty<AppAttribute>())
            Attributes.Add(attribute);
        ReturnedCode = libraryReturnedCodes;
    }

}
