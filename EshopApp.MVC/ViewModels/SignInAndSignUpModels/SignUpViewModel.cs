using EshopApp.MVC.CustomValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.SignInAndSignUpModels;

public class SignUpViewModel
{
    [Required(ErrorMessage = "This field is required")]
    [EmailAddress]
    public string? Email { get; set; }

    [Phone]
    [RegularExpression(@"^\+?\d{1,4}[\s\-]?\(?\d{1,3}\)?[\s\-]?\d{1,4}[\s\-]?\d{1,4}[\s\-]?\d{1,4}$", ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [MaxLength(128, ErrorMessage = "Password can not exceed 128 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$",
        ErrorMessage = "Password must include at least an uppercase letter, a lowercase letter, a digit, and a special character")]
    [ComparePassword("RepeatPassword", ErrorMessage = "The passwords do not match")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "This field is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [MaxLength(128, ErrorMessage = "Password can not exceed 128 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$",
        ErrorMessage = "Password must include at least an uppercase letter, a lowercase letter, a digit, and a special character")]
    [ComparePassword("Password", ErrorMessage = "The passwords do not match")]
    [Display(Name = "Repeat Password")]
    public string? RepeatPassword { get; set; }
}
