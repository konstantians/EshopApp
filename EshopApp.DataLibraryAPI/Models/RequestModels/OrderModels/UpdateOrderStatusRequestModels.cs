using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.OrderModels;

public class UpdateOrderStatusRequestModels
{
    [Required]
    [MaxLength(50)]
    [RegularExpression("Pending|Confirmed|Processed|Canceled|Shipped|NoShow|Completed|Refunded|Failed",
    ErrorMessage = "The trigger event must have one of the following values: OnSignUp, OnFirstOrder, OnEveryFiveOrders, OnEveryTenOrders, NoTrigger.")]
    public string? NewOrderStatus { get; set; }
    [Required]
    [MaxLength(50)]
    public string? OrderId { get; set; }
}
