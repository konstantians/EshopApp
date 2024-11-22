namespace EshopApp.DataLibrary.Models.ResponseModels.CouponModels;
public class ReturnUserCouponsAndCodeResponseModel
{
    public List<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnUserCouponsAndCodeResponseModel() { }
    public ReturnUserCouponsAndCodeResponseModel(List<UserCoupon> userCoupons, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var userCoupon in userCoupons ?? Enumerable.Empty<UserCoupon>())
            UserCoupons.Add(userCoupon);
        ReturnedCode = libraryReturnedCodes;
    }

}
