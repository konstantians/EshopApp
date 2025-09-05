using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.CreateProductModels;

public class CreateVariantViewModel
{
    [Required]
    [MaxLength(50)]
    public string? SKU { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The price of the variant can not be negative.")]
    public decimal? Price { get; set; }
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "The units in stock value can not be negative.")]
    public int? UnitsInStock { get; set; }
    public bool IsThumbnailVariant { get; set; }
    public bool IsActivated { get; set; } = true;
    public bool IsDeactivated { get; set; }
    public bool ExistsInOrder { get; set; }
    [Required]
    [MaxLength(50)]
    public string? ProductId { get; set; }
    [MaxLength(50)]
    public string? DiscountId { get; set; }
    public List<string>? AttributeIds { get; set; }
    public string? ImageIds { get; set; }
    [MaxLength(50)]
    public string? ImageIdThatShouldBeThumbnail { get; set; }
    public List<CreateVariantImageViewModel>? VariantImageRequestModels { get; set; }

}
