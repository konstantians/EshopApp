using Microsoft.Playwright;

namespace EshopApp.EToETests.Pages;
internal class ResetPasswordPage
{
    private readonly IPage _page;
    private readonly ILocator passwordResetHeader;
    private readonly ILocator passwordField;
    private readonly ILocator confirmPasswordField;
    private readonly ILocator resetPasswordButton;

    public ResetPasswordPage(IPage page)
    {
        _page = page;

        passwordResetHeader = _page.GetByTestId("passwordResetHeader");
        passwordField = _page.GetByTestId("passwordInput");
        confirmPasswordField = _page.GetByTestId("confirmPasswordInput");
        resetPasswordButton = _page.GetByTestId("resetPasswordButton");
    }


    public async Task SubmitResetPasswordForm(string password = "Kinas2016!", string confirmPassword = "Kinas2016!", bool clientError = false)
    {
        await passwordField.FillAsync(password);
        if (clientError)
            await confirmPasswordField.FillAsync(confirmPassword + "a");
        else
            await confirmPasswordField.FillAsync(confirmPassword);

        await resetPasswordButton.ClickAsync();

        await Task.Delay(300);
    }

    public async Task<bool> IsPageShown()
    {
        return await passwordResetHeader.IsVisibleAsync();
    }

}
