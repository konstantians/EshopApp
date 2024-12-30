namespace EshopApp.TransactionLibrary.Models.ResponseModels;
public class ReturnSessionIdSessionUrlAndCodeResponseModel
{
    public string? CheckOutSessionId { get; set; }
    public string? CheckOutSessionUrl { get; set; }
    public TransactionLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnSessionIdSessionUrlAndCodeResponseModel() { }
    public ReturnSessionIdSessionUrlAndCodeResponseModel(string checkOutSessionId, string checkOutSessionUrl, TransactionLibraryReturnedCodes returnedCode)
    {
        CheckOutSessionId = checkOutSessionId;
        CheckOutSessionUrl = checkOutSessionUrl;
        ReturnedCode = returnedCode;
    }
}
