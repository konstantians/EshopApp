using EshopApp.DataLibraryAPI.Models.RequestModels.VariantImageModels;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.VariantModels;

public class CreateVariantRequestModel
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
    public bool? IsThumbnailVariant { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    [Required]
    [MaxLength(50)]
    public string? ProductId { get; set; }
    [MaxLength(50)]
    public string? DiscountId { get; set; }
    //TODO can not validate individually each stupid element maybe do something about it in the future..
    public List<string> AttributeIds { get; set; } = new List<string>();
    public List<CreateVariantImageRequestModel> VariantImageRequestModels { get; set; } = new List<CreateVariantImageRequestModel>();
}
