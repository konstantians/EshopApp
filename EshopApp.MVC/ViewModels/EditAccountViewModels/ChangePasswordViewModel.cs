using EshopApp.MVC.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.EditAccountViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "This field is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [MaxLength(128, ErrorMessage = "Password can not exceed 128 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$",
        ErrorMessage = "Password must include at least an uppercase letter, a lowercase letter, a digit, and a special character")]
    public string? OldPassword { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [MaxLength(128, ErrorMessage = "Password can not exceed 128 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$",
        ErrorMessage = "Password must include at least an uppercase letter, a lowercase letter, a digit, and a special character")]
    [ComparePassword("ConfirmNewPassword", ErrorMessage = "The passwords do not match.")]
    public string? NewPassword { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [MaxLength(128, ErrorMessage = "Password can not exceed 128 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$",
        ErrorMessage = "Password must include at least an uppercase letter, a lowercase letter, a digit, and a special character")]
    [ComparePassword("NewPassword", ErrorMessage = "The passwords do not match")]
    [Display(Name = "Confirm New Password")]
    public string? ConfirmNewPassword { get; set; }
}
