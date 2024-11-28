namespace EshopApp.DataLibrary.Models.ResponseModels.PaymentOptionModels;
public class ReturnPaymentOptionsAndCodeResponseModel
{
    public List<PaymentOption> PaymentOptions { get; set; } = new List<PaymentOption>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnPaymentOptionsAndCodeResponseModel() { }
    public ReturnPaymentOptionsAndCodeResponseModel(List<PaymentOption> paymentOptions, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var paymentOption in paymentOptions ?? Enumerable.Empty<PaymentOption>())
            PaymentOptions.Add(paymentOption);
        ReturnedCode = libraryReturnedCodes;
    }
}
