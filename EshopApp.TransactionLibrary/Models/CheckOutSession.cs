namespace EshopApp.TransactionLibrary.Models;
public class CheckOutSession
{
    public string? PaymentMethodType { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
    public string? CustomerEmail { get; set; }
    public int? CouponPercentage { get; set; } //optional
    public DateTime ExpiresAt { get; set; } = DateTime.Now.AddDays(1);
    public List<TransactionOrderItem> TransactionOrderItems { get; set; } = new List<TransactionOrderItem>(); //there needs to be at least one order item
    public TransactionPaymentOption? TransactionPaymentOption { get; set; }
    public TransactionShippingOption? TransactionShippingOption { get; set; }
}
