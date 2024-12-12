using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CouponModels;

namespace EshopApp.DataLibrary.DataAccess;
public interface ICouponDataAccess
{
    Task<ReturnUserCouponAndCodeResponseModel> AddCouponToUserAsync(UserCoupon userCoupon);
    Task<ReturnCouponAndCodeResponseModel> CreateCouponAsync(Coupon coupon);
    Task<DataLibraryReturnedCodes> DeleteCouponAsync(string couponId);
    Task<ReturnCouponAndCodeResponseModel> GetCouponByIdAsync(string id, bool includeDeactivated);
    Task<ReturnCouponsAndCodeResponseModel> GetCouponsAsync(int amount, bool includeDeactivated);
    Task<ReturnUserCouponsAndCodeResponseModel> GetCouponsOfUserAsync(string userId, bool includeDeactivated);
    Task<DataLibraryReturnedCodes> RemoveAllCouponsOfUser(string userId);
    Task<DataLibraryReturnedCodes> RemoveCouponFromUser(string userCouponId);
    Task<DataLibraryReturnedCodes> UpdateCouponAsync(Coupon updatedCoupon);
    Task<DataLibraryReturnedCodes> UpdateUserCouponAsync(UserCoupon updatedUserCoupon);
}