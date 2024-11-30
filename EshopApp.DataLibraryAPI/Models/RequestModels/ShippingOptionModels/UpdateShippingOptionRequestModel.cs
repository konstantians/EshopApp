using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.ShippingOptionModels;

public class UpdateShippingOptionRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The extra cost property must be a non-negative integer.")]
    public decimal? ExtraCost { get; set; }
    public bool? ContainsDelivery { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public List<string>? OrderIds { get; set; }
}
