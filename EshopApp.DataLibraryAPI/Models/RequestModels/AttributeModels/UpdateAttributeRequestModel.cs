using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.AttributeModels;

public class UpdateAttributeRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    public List<string>? VariantIds { get; set; }
}
