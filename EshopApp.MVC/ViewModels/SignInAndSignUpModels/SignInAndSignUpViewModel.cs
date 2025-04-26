namespace EshopApp.MVC.ViewModels.SignInAndSignUpModels;

public class SignInAndSignUpViewModel
{
    public SignInViewModel SignInViewModel { get; set; } = new SignInViewModel();
    public SignUpViewModel SignUpViewModel { get; set; } = new SignUpViewModel();
    public ForgotPasswordViewModel ForgotPasswordViewModel { get; set; } = new ForgotPasswordViewModel();
}
