namespace EshopApp.DataLibrary.Models.ResponseModels.ImagesModels;
public class ReturnImagesAndCodeResponseModel
{
    public List<AppImage> Images { get; set; } = new List<AppImage>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnImagesAndCodeResponseModel() { }
    public ReturnImagesAndCodeResponseModel(List<AppImage> images, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var image in images ?? Enumerable.Empty<AppImage>())
            Images.Add(image);
        ReturnedCode = libraryReturnedCodes;
    }
}
