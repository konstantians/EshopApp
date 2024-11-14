using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CategoryModels;

public class CreateCategoryRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
}
