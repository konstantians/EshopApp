using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CategoryModels;

public class UpdateCategoryRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    public List<string>? ProductIds { get; set; }
}
