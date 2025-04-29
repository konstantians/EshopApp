using Microsoft.Playwright;

namespace EshopApp.EToETests.Pages;
internal class EditAccountPage
{
    private readonly IPage _page;
    private readonly ILocator basicSettingsHeader;
    private readonly ILocator sensitiveSettingsHeader;

    private readonly ILocator firstNameEditButton;
    private readonly ILocator firstNameInput;
    private readonly ILocator lastNameEditButton;
    private readonly ILocator lastNameInput;
    private readonly ILocator phoneNumberEditButton;
    private readonly ILocator phoneNumberInput;
    private readonly ILocator changeBasicAccountSettingsButton;

    private readonly ILocator changePasswordModalTriggerButton;
    private readonly ILocator oldPasswordInput;
    private readonly ILocator newPasswordInput;
    private readonly ILocator confirmNewPasswordInput;
    private readonly ILocator cancelChangePasswordButton;
    private readonly ILocator changePasswordButton;

    private readonly ILocator changeEmailModalTriggerButton;
    private readonly ILocator newEmailInput;
    private readonly ILocator cancelChangeEmailButton;
    private readonly ILocator changeEmailButton;

    private readonly ILocator deleteAccountModalTriggerButton;
    private readonly ILocator emailInput;
    private readonly ILocator cancelDeleteAccountButton;
    private readonly ILocator deleteAccountButton;

    public EditAccountPage(IPage page)
    {
        _page = page;
        basicSettingsHeader = _page.GetByTestId("basicSettingsHeader");
        sensitiveSettingsHeader = _page.GetByTestId("sensitiveSettingsHeader");

        firstNameInput = _page.GetByTestId("firstNameInput");
        firstNameEditButton = _page.GetByTestId("firstNameEditButton");
        lastNameInput = _page.GetByTestId("lastNameInput");
        lastNameEditButton = _page.GetByTestId("lastNameEditButton");
        phoneNumberInput = _page.GetByTestId("phoneNumberInput");
        phoneNumberEditButton = _page.GetByTestId("phoneNumberEditButton");
        changeBasicAccountSettingsButton = _page.GetByTestId("changeBasicAccountSettingsButton");

        changePasswordModalTriggerButton = _page.GetByTestId("changePasswordModalTriggerButton");
        oldPasswordInput = _page.GetByTestId("oldPasswordInput");
        newPasswordInput = _page.GetByTestId("newPasswordInput");
        confirmNewPasswordInput = _page.GetByTestId("confirmNewPasswordInput");
        cancelChangePasswordButton = _page.GetByTestId("cancelChangePasswordButton");
        changePasswordButton = _page.GetByTestId("changePasswordButton");

        changeEmailModalTriggerButton = _page.GetByTestId("changeEmailModalTriggerButton");
        newEmailInput = _page.GetByTestId("newEmailInput");
        cancelChangeEmailButton = _page.GetByTestId("cancelChangeEmailButton");
        changeEmailButton = _page.GetByTestId("changeEmailButton");

        deleteAccountModalTriggerButton = _page.GetByTestId("deleteAccountModalTriggerButton");
        emailInput = _page.GetByTestId("emailInput");
        cancelDeleteAccountButton = _page.GetByTestId("cancelDeleteAccountButton");
        deleteAccountButton = _page.GetByTestId("deleteEmailButton");
    }

    public async Task NavigateToPage()
    {
        await _page.GotoAsync("https://localhost:7070/Account/EditAccount");
        await Task.Delay(300);
    }

    public async Task ChangeAccountBasicSettings(string firstName, string lastName, string phoneNumber, bool clientError = false)
    {
        await firstNameEditButton.ClickAsync();
        if (!clientError)
            await firstNameInput.FillAsync(firstName);
        await firstNameEditButton.ClickAsync();

        await lastNameEditButton.ClickAsync();
        if (!clientError)
            await lastNameInput.FillAsync(lastName);
        await lastNameEditButton.ClickAsync();

        await phoneNumberEditButton.ClickAsync();
        if (clientError)
            await phoneNumberInput.FillAsync("invalid phone format");
        else
            await phoneNumberInput.FillAsync(phoneNumber);
        await phoneNumberEditButton.ClickAsync();

        await changeBasicAccountSettingsButton.ClickAsync();

        await Task.Delay(300);
    }

    public async Task ChangeAccountPassword(string oldPassword = "Kinas2016!", string newPassword = "Kinas2020!",
        string confirmNewPassword = "Kinas2020!", bool clientError = false)
    {
        await changePasswordModalTriggerButton.ClickAsync();
        if (clientError)
        {
            await oldPasswordInput.FillAsync("");
            await newPasswordInput.FillAsync("");
            await confirmNewPasswordInput.FillAsync("");
        }
        else
        {
            await oldPasswordInput.FillAsync(oldPassword);
            await newPasswordInput.FillAsync(newPassword);
            await confirmNewPasswordInput.FillAsync(confirmNewPassword);
        }

        await changePasswordButton.ClickAsync();

        await Task.Delay(300);
    }

    public async Task ChangeAccountEmail(string newEmail = "realag58@gmail.com", bool clientError = false)
    {
        await changeEmailModalTriggerButton.ClickAsync();
        if (clientError)
            await newEmailInput.FillAsync("");
        else
            await newEmailInput.FillAsync(newEmail);

        await changeEmailButton.ClickAsync();

        await Task.Delay(300);
    }

    public async Task DeleteAccount(string email = "kinnaskonstantinos0@gmail.com", bool clientError = false)
    {
        await deleteAccountModalTriggerButton.ClickAsync();
        if (clientError)
            await emailInput.FillAsync("");
        else
            await emailInput.FillAsync(email);

        await deleteAccountButton.ClickAsync();

        await Task.Delay(300);
    }

    public async Task<bool> IsPageShown()
    {
        return await sensitiveSettingsHeader.IsVisibleAsync() || await basicSettingsHeader.IsVisibleAsync();
    }
}
