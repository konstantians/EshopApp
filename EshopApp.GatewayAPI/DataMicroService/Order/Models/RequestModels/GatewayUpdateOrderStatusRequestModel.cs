using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Order.Models.RequestModels;

public class GatewayUpdateOrderStatusRequestModel
{
    [Required]
    [MaxLength(50)]
    [RegularExpression("Pending|Confirmed|Processed|Canceled|Shipped|NoShow|Completed|RefundPending|RefundFailed|Refunded|Failed",
    ErrorMessage = "The trigger event must have one of the following values: OnSignUp, OnFirstOrder, OnEveryFiveOrders, OnEveryTenOrders, NoTrigger.")]
    public string? NewOrderStatus { get; set; }
    [Required]
    [MaxLength(50)]
    public string? OrderId { get; set; }
}
