namespace EshopApp.TransactionLibrary.Models.ResponseModels;
public class ReturnCheckOutSessionIdAndCodeResponseModel
{
    public string? CheckOutSessionId { get; set; }
    public TransactionLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCheckOutSessionIdAndCodeResponseModel() { }
    public ReturnCheckOutSessionIdAndCodeResponseModel(string checkOutSessionId, TransactionLibraryReturnedCodes returnedCode)
    {
        CheckOutSessionId = checkOutSessionId;
        ReturnedCode = returnedCode;
    }
}
