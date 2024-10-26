namespace EshopApp.DataLibrary.Models.ResponseModels.ImagesModels;
public class ReturnImageAndCodeResponseModel
{
    public AppImage? Image { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnImageAndCodeResponseModel() { }
    public ReturnImageAndCodeResponseModel(AppImage image, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Image = image;
        ReturnedCode = libraryReturnedCodes;
    }
}
