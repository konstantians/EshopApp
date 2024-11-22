namespace EshopApp.DataLibrary.Models.ResponseModels.CouponModels;
public class ReturnCouponsAndCodeResponseModel
{
    public List<Coupon> Coupons { get; set; } = new List<Coupon>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCouponsAndCodeResponseModel() { }
    public ReturnCouponsAndCodeResponseModel(List<Coupon> coupons, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var coupon in coupons ?? Enumerable.Empty<Coupon>())
            Coupons.Add(coupon);
        ReturnedCode = libraryReturnedCodes;
    }
}
