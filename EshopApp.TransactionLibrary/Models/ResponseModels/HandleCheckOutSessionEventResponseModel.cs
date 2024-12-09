namespace EshopApp.TransactionLibrary.Models.ResponseModels;
public class HandleCheckOutSessionEventResponseModel
{
    public string? CheckOutSessionId { get; set; }
    public string? CheckOutSessionUrl { get; set; }
    public TransactionLibraryReturnedCodes ReturnedCode { get; set; }

    public HandleCheckOutSessionEventResponseModel() { }
    public HandleCheckOutSessionEventResponseModel(string checkOutSessionId, string checkOutSessionUrl, TransactionLibraryReturnedCodes returnedCode)
    {
        CheckOutSessionId = checkOutSessionId;
        CheckOutSessionUrl = checkOutSessionUrl;
        ReturnedCode = returnedCode;
    }
}
