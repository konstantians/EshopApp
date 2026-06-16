using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.OrderPlacementModels;

public class CartItemValidationModel
{
    public string? CartItemId { get; set; }
    [Required]
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public int UnitsInStockAtCurrentMoment { get; set; }
}
