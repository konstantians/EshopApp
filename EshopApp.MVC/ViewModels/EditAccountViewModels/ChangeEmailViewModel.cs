using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.EditAccountViewModels;

public class ChangeEmailViewModel
{
    [EmailAddress]
    public string? OldEmail { get; set; }
    [Required(ErrorMessage = "This field is required")]
    [EmailAddress]
    public string? NewEmail { get; set; }
}
