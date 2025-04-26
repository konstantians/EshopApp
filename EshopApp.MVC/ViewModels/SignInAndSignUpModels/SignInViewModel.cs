using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.SignInAndSignUpModels;

public class SignInViewModel
{
    [Required(ErrorMessage = "This field is required")]
    [EmailAddress]
    public string? Email { get; set; }
    [Required(ErrorMessage = "This field is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [MaxLength(128, ErrorMessage = "Password can not exceed 128 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).*$",
        ErrorMessage = "Password must include at least an uppercase letter, a lowercase letter, a digit, and a special character")]
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
