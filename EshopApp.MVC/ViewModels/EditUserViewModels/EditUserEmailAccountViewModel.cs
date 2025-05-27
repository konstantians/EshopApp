using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.EditUserViewModels;

public class EditUserEmailAccountViewModel
{
    [Required]
    public string? UserId { get; set; }
    [EmailAddress]
    public string? OldEmail { get; set; }
    [Required(ErrorMessage = "This field is required")]
    [EmailAddress]
    public string? NewEmail { get; set; }
}
