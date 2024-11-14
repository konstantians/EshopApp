using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.AttributeModels;

public class CreateAttributeRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
}
