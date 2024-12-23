using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Variant.Models.RequestModels;

public class GatewayUpdateVariantRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? SKU { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The price of the variant can not be negative.")]
    public decimal? Price { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The units in stock value can not be negative.")]
    public int? UnitsInStock { get; set; }
    public bool? IsThumbnailVariant { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    [MaxLength(50)]
    public string? DiscountId { get; set; }
    public List<string>? AttributeIds { get; set; }
    public List<string>? ImagesIds { get; set; }
    [MaxLength(50)]
    public string? ImageIdThatShouldBeThumbnail { get; set; }
}
