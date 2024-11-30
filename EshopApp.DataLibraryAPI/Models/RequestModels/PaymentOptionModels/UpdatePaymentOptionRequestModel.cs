using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.PaymentOptionModels;

public class UpdatePaymentOptionRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    [MaxLength(50)]
    public string? NameAlias { get; set; }
    public string? Description { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The extra cost property must be a non-negative integer.")]
    public decimal? ExtraCost { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
    public List<string>? PaymentDetailsIds { get; set; }
}
