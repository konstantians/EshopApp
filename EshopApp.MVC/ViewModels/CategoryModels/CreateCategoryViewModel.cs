using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.CategoryModels;

public class CreateCategoryViewModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    public List<string>? ProductIds { get; set; }
}
