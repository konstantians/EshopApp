namespace EshopApp.TransactionLibraryAPI.Models.ResponseModels;

public class HandleIssueRefundEventResponseModel
{
    public string? NewOrderState { get; set; }
    public string? PaymentIntentId { get; set; }
    public bool ShouldSendEmail { get; set; }
}
