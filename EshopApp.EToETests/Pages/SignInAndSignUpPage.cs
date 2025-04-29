using Microsoft.Playwright;

namespace EshopApp.EToETests.Pages;
internal class SignInAndSignUpPage
{
    private readonly IPage _page;
    private readonly ILocator signInHeader;
    private readonly ILocator signInEmailField;
    private readonly ILocator signInPasswordField;
    private readonly ILocator signInButton;
    private readonly ILocator getToSignInFormButton;

    private readonly ILocator signUpEmailField;
    private readonly ILocator signUpPhoneNumberField;
    private readonly ILocator signUpPasswordField;
    private readonly ILocator signUpRepeatPasswordField;
    private readonly ILocator signUpButton;
    private readonly ILocator getToSignUpFormButton;

    private readonly ILocator forgotPasswordLink;
    private readonly ILocator forgotPasswordEmailField;
    private readonly ILocator forgotPasswordConfirmButton;
    private readonly ILocator forgotPasswordCloseModalButton;

    public SignInAndSignUpPage(IPage page)
    {
        _page = page;

        signInHeader = _page.GetByTestId("signInHeader");
        signInEmailField = _page.GetByTestId("signInEmailInput");
        signInPasswordField = _page.GetByTestId("signInPasswordInput");
        getToSignInFormButton = _page.GetByTestId("getToSignInFormButton");
        signInButton = _page.GetByTestId("signInButton");

        signUpEmailField = _page.GetByTestId("signUpEmailInput");
        signUpPhoneNumberField = _page.GetByTestId("signUpPhoneNumberInput");
        signUpPasswordField = _page.GetByTestId("signUpPasswordInput");
        signUpRepeatPasswordField = _page.GetByTestId("signUpRepeatPasswordInput");
        getToSignUpFormButton = _page.GetByTestId("getToSignUpFormButton");
        signUpButton = _page.GetByTestId("signUpButton");

        forgotPasswordLink = _page.GetByTestId("forgotPasswordLink");
        forgotPasswordEmailField = _page.GetByTestId("forgotPasswordEmailField");
        forgotPasswordConfirmButton = _page.GetByTestId("forgotPasswordConfirmButton");
        forgotPasswordCloseModalButton = _page.GetByTestId("forgotPasswordCloseModalButton");
    }

    public async Task NavigateToPage()
    {
        await _page.GotoAsync("https://localhost:7070/Account/SignInAndSignUp");
        await Task.Delay(300);
    }

    public async Task SubmitSignInForm(string email = "kinnaskonstantinos0@gmail.com", string password = "Kinas2016!", bool isOnSignInSide = false, bool hasClientError = false)
    {
        if (!isOnSignInSide)
            await getToSignInFormButton.ClickAsync();

        await signInEmailField.FillAsync(email);

        if (hasClientError)
            await signInPasswordField.FillAsync("");
        else
            await signInPasswordField.FillAsync(password);

        await signInButton.ClickAsync();
        await Task.Delay(300);
    }

    public async Task SubmitSignUpForm(string email = "kinnaskonstantinos0@gmail.com", string phoneNumber = "6943655624", string password = "Kinas2016!", string repeatPassword = "Kinas2016!",
        bool isOnSignUpSide = false, bool hasClientError = false)
    {
        if (!isOnSignUpSide)
            await getToSignUpFormButton.ClickAsync();

        await signUpEmailField.FillAsync(email);
        await signUpPhoneNumberField.FillAsync(phoneNumber);
        await signUpPasswordField.FillAsync(password);
        if (hasClientError)
            await signUpRepeatPasswordField.FillAsync(repeatPassword + "a");
        else
            await signUpRepeatPasswordField.FillAsync(repeatPassword);

        await signUpButton!.ClickAsync();
        await Task.Delay(300);
    }

    public async Task ForgotPassword(string email = "kinnaskonstantinos0@gmail.com", bool isOnSignInSide = false, bool hasClientError = false)
    {
        if (!isOnSignInSide)
            await getToSignInFormButton.ClickAsync();

        await forgotPasswordLink.ClickAsync();

        if (hasClientError)
        {
            await forgotPasswordConfirmButton.ClickAsync();
            await forgotPasswordCloseModalButton.ClickAsync();
            return;
        }

        await forgotPasswordEmailField.FillAsync(email);
        await forgotPasswordConfirmButton.ClickAsync();
        await Task.Delay(300);
    }

    public async Task<bool> IsPageShown()
    {
        return await signInHeader.IsVisibleAsync();
    }
}
