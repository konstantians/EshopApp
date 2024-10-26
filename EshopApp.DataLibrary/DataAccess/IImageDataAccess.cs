using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ImagesModels;

namespace EshopApp.DataLibrary.DataAccess;
public interface IImageDataAccess
{
    Task<ReturnImageAndCodeResponseModel> CreateImageAsync(AppImage image);
    Task<DataLibraryReturnedCodes> DeleteImageAsync(string imageId);
    Task<ReturnImageAndCodeResponseModel> GetImageAsync(string id, bool includeSoftDeletedImages);
    Task<ReturnImagesAndCodeResponseModel> GetImagesAsync(int amount, bool includeSoftDeletedImages);
    Task<DataLibraryReturnedCodes> RestoreSoftDeletedImagesAsync(List<string> imageIds);
    Task<DataLibraryReturnedCodes> UpdateImageAsync(AppImage updatedImage);
}