namespace EshopApp.DataLibrary.Models.ResponseModels.CouponModels;
public class ReturnUserCouponAndCodeResponseModel
{
    public UserCoupon? UserCoupon { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnUserCouponAndCodeResponseModel() { }
    public ReturnUserCouponAndCodeResponseModel(UserCoupon userCoupon, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        UserCoupon = userCoupon;
        ReturnedCode = libraryReturnedCodes;
    }

}
