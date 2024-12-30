namespace EshopApp.GatewayAPI.DataMicroService.SharedModels;

public class GatewayPaymentOption
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    public decimal? ExtraCost { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public List<GatewayPaymentDetails> PaymentDetails { get; set; } = new List<GatewayPaymentDetails>();
}
