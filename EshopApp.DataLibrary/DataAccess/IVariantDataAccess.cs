using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.VariantModels;

namespace EshopApp.DataLibrary.DataAccess
{
    public interface IVariantDataAccess
    {
        Task<ReturnVariantAndCodeResponseModel> CreateVariantAsync(Variant variant);
        Task<ReturnVariantImageAndCodeResponseModel> CreateVariantImageAsync(VariantImage variantImage);
        Task<DataLibraryReturnedCodes> DeleteVariantAsync(string variantId);
        Task<DataLibraryReturnedCodes> DeleteVariantImageAsync(string variantImageId);
        Task<ReturnVariantAndCodeResponseModel> GetVariantByIdAsync(string id);
        Task<ReturnVariantAndCodeResponseModel> GetVariantBySKUAsync(string sku);
        Task<ReturnVariantAndCodeResponseModel> GetVariantImageByIdAsync(string id);
        Task<ReturnVariantsImagesAndCodeResponseModel> GetVariantImagesAsync(int amount);
        Task<ReturnVariantsAndCodeResponseModel> GetVariantsAsync(int amount);
        Task<DataLibraryReturnedCodes> UpdateVariantAsync(Variant updatedVariant);
        Task<DataLibraryReturnedCodes> UpdateVariantImageAsync(VariantImage updatedVariantImage);
    }
}