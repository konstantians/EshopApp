using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.CreateProductModels;

public class CreateVariantImageViewModel
{
    public bool IsThumbNail { get; set; }
    [Required]
    [MaxLength(50)]
    public string? ImageId { get; set; }
}
