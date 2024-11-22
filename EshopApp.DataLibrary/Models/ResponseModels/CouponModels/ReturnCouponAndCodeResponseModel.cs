namespace EshopApp.DataLibrary.Models.ResponseModels.CouponModels;
public class ReturnCouponAndCodeResponseModel
{
    public Coupon? Coupon { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCouponAndCodeResponseModel() { }
    public ReturnCouponAndCodeResponseModel(Coupon coupon, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Coupon = coupon;
        ReturnedCode = libraryReturnedCodes;
    }

}
