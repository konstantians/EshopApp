using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ShippingOptionModels;

namespace EshopApp.DataLibrary.DataAccess;
public interface IShippingOptionDataAccess
{
    Task<ReturnShippingOptionAndCodeResponseModel> CreateShippingOptionAsync(ShippingOption shippingOption);
    Task<DataLibraryReturnedCodes> DeleteShippingOptionAsync(string shippingOptionId);
    Task<ReturnShippingOptionAndCodeResponseModel> GetShippingOptionByIdAsync(string id, bool includeDeactivated);
    Task<ReturnShippingOptionsAndCodeResponseModel> GetShippingOptionsAsync(int amount, bool includeDeactivated);
    Task<DataLibraryReturnedCodes> UpdateShippingOptionAsync(ShippingOption updatedShippingOption);
}