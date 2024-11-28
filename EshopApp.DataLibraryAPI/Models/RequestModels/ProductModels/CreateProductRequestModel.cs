using EshopApp.DataLibraryAPI.Models.RequestModels.VariantModels;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.ProductModels;

public class CreateProductRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Code { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string> CategoryIds { get; set; } = new List<string>();
    [Required]
    public CreateVariantRequestModel? CreateVariantRequestModel { get; set; }
}
