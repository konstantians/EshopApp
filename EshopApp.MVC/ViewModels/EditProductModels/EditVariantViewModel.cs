using EshopApp.MVC.Models.DataModels;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.EditProductModels;

public class EditVariantViewModel
{
    [Required()]
    [MaxLength(50)]
    public string? Id { get; set; }
    [Required()]
    [MaxLength(50)]
    public string? ProductId { get; set; }
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
    public UiDiscount? Discount { get; set; }
    [MaxLength(50)]
    public string? DiscountId { get; set; }
    public List<UiImage>? Images { get; set; }
    public List<string>? ImagesIds { get; set; }
    public List<string>? AttributeIds { get; set; }
    [MaxLength(50)]
    public string? ImageIdThatShouldBeThumbnail { get; set; }
}
