using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.SignInAndSignUpModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "This field is required")]
    [EmailAddress]
    public string? RecoveryEmail { get; set; }
}
