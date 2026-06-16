using EshopApp.MVC.ViewModels.SignInAndSignUpModels;

namespace EshopApp.MVC.ViewModels.OrderPlacementModels;

public class SignInAndForgotPasswordViewModel
{
    public SignInViewModel SignInViewModel { get; set; } = new SignInViewModel();
    public ForgotPasswordViewModel ForgotPasswordViewModel { get; set; } = new ForgotPasswordViewModel();
}
