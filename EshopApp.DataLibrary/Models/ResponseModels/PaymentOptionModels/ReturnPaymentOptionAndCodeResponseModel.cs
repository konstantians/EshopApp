namespace EshopApp.DataLibrary.Models.ResponseModels.PaymentOptionModels;
public class ReturnPaymentOptionAndCodeResponseModel
{
    public PaymentOption? PaymentOption { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnPaymentOptionAndCodeResponseModel() { }
    public ReturnPaymentOptionAndCodeResponseModel(PaymentOption paymentOption, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        PaymentOption = paymentOption;
        ReturnedCode = libraryReturnedCodes;
    }
}