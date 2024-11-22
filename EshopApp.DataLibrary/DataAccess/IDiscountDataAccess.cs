using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.DiscountModels;

namespace EshopApp.DataLibrary.DataAccess;

public interface IDiscountDataAccess
{
    Task<ReturnDiscountAndCodeResponseModel> CreateDiscountAsync(Discount discount);
    Task<DataLibraryReturnedCodes> DeleteDiscountAsync(string discountId);
    Task<ReturnDiscountAndCodeResponseModel> GetDiscountByIdAsync(string id, bool includeDeactivated);
    Task<ReturnDiscountsAndCodeResponseModel> GetDiscountsAsync(int amount, bool includeDeactivated);
    Task<DataLibraryReturnedCodes> UpdateDiscountAsync(Discount updatedDiscount);
}