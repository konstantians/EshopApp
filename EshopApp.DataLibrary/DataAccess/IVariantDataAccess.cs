using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.VariantModels;

namespace EshopApp.DataLibrary.DataAccess;

public interface IVariantDataAccess
{
    Task<ReturnVariantAndCodeResponseModel> CreateVariantAsync(Variant variant);
    Task<DataLibraryReturnedCodes> DeleteVariantAsync(string variantId);
    Task<ReturnVariantAndCodeResponseModel> GetVariantByIdAsync(string id, bool includeDeactivated);
    Task<ReturnVariantAndCodeResponseModel> GetVariantBySKUAsync(string sku, bool includeDeactivated);
    Task<ReturnVariantsAndCodeResponseModel> GetVariantsAsync(int amount, bool includeDeactivated);
    Task<DataLibraryReturnedCodes> UpdateVariantAsync(Variant updatedVariant);
}