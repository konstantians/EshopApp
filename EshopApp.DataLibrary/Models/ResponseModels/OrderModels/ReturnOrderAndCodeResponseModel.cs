namespace EshopApp.DataLibrary.Models.ResponseModels.OrderModels;
public class ReturnOrderAndCodeResponseModel
{
    public Order? Order { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnOrderAndCodeResponseModel() { }
    public ReturnOrderAndCodeResponseModel(Order order, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Order = order;
        ReturnedCode = libraryReturnedCodes;
    }
}
