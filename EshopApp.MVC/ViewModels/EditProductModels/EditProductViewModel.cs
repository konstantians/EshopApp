using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.EditProductModels;

public class EditProductViewModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
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
}