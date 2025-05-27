namespace EshopApp.MVC.ViewModels.EditUserViewModels;

public class EditUserViewModel
{
    public EditUserAccountBasicSettingsViewModel EditUserAccountBasicSettingsViewModel { get; set; } = new EditUserAccountBasicSettingsViewModel();
    public EditUserPasswordViewModel EditUserPasswordViewModel { get; set; } = new EditUserPasswordViewModel();
    public EditUserRoleViewModel EditUserRoleViewModel { get; set; } = new EditUserRoleViewModel();
    public EditUserEmailAccountViewModel EditUserEmailAccountViewModel { get; set; } = new EditUserEmailAccountViewModel();
}
