namespace EshopApp.DataLibrary.Models.ResponseModels.AttributeModels;

public class ReturnAttributeAndCodeResponseModel
{
    public AppAttribute? Attribute { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnAttributeAndCodeResponseModel() { }
    public ReturnAttributeAndCodeResponseModel(AppAttribute attribute, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Attribute = attribute;
        ReturnedCode = libraryReturnedCodes;
    }
}
