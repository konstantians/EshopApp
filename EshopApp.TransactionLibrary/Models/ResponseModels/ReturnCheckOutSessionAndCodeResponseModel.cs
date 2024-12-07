namespace EshopApp.TransactionLibrary.Models.ResponseModels;
public class ReturnCheckOutSessionAndCodeResponseModel
{
    public CheckOutSession? CheckOutSession { get; set; }
    public TransactionLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCheckOutSessionAndCodeResponseModel() { }
    public ReturnCheckOutSessionAndCodeResponseModel(CheckOutSession checkOutSession, TransactionLibraryReturnedCodes returnedCode)
    {
        CheckOutSession = checkOutSession;
        ReturnedCode = returnedCode;
    }
}
