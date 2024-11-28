namespace EshopApp.DataLibrary.Models.ResponseModels.OrderModels;
public class ReturnOrdersAndCodeResponseModel
{
    public List<Order> Orders { get; set; } = new List<Order>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnOrdersAndCodeResponseModel() { }
    public ReturnOrdersAndCodeResponseModel(List<Order> orders, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var order in orders ?? Enumerable.Empty<Order>())
            Orders.Add(order);
        ReturnedCode = libraryReturnedCodes;
    }

}
