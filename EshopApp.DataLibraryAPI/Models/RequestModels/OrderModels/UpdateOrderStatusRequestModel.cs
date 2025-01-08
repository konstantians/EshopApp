using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.OrderModels;

public class UpdateOrderStatusRequestModel
{
    [Required]
    [MaxLength(50)]
    [RegularExpression("Pending|Confirmed|Processed|Canceled|Shipped|NoShow|Completed|RefundPending|RefundFailed|Refunded|Failed",
    ErrorMessage = "The trigger event must have one of the following values: OnSignUp, OnFirstOrder, OnEveryFiveOrders, OnEveryTenOrders, NoTrigger.")]
    public string? NewOrderStatus { get; set; }
    [MaxLength(50)]
    public string? OrderId { get; set; }
    [MaxLength(100)]
    public string? PaymentProcessorSessionId { get; set; }
}
