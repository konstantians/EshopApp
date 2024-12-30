namespace EshopApp.GatewayAPI.TransactionMicroService.CheckOutSession.Models.ServiceRequestModels;

public class GatewayCreateTransactionOrderItemServiceRequestModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? FinalUnitAmountInEuro { get; set; } //We will need to multiply by 100, because Stripe works with cents
    public int? Quantity { get; set; }
}
