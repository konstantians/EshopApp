namespace EshopApp.MVC.ViewModels.EditAccountViewModels;

public class EditAccountViewModel
{
    public ChangeAccountBasicSettingsViewModel ChangeAccountBasicSettings { get; set; } = new ChangeAccountBasicSettingsViewModel();
    public ChangePasswordViewModel ChangePasswordViewModel { get; set; } = new ChangePasswordViewModel();
    public ChangeEmailViewModel ChangeEmailViewModel { get; set; } = new ChangeEmailViewModel();
}
