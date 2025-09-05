using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.CreateProductModels;

public class CreateProductViewModel
{

    [Required]
    [MaxLength(50)]
    public string? Code { get; set; }
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActivated { get; set; }
    public bool IsDeactivated { get; set; }
    public List<string>? CategoryIds { get; set; }
    [Required]
    public CreateVariantViewModel? CreateVariantRequestModel { get; set; }
}
