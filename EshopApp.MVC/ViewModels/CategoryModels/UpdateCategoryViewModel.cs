using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.CategoryModels;

public class UpdateCategoryViewModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    [Required]
    [MaxLength(50)]
    public string? CategoryIconLink { get; set; }
    public List<string>? ProductIds { get; set; }
}
