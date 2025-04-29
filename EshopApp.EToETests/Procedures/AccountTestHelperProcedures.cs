using EshopApp.EToETests.Pages;
using Microsoft.Playwright;

namespace EshopApp.EToETests.Procedures;
internal class AccountTestHelperProcedures : IAccountTestHelperProcedures
{
    private readonly PlaywrightTest _playWright;
    private readonly IPage _page;
    private SignInAndSignUpPage _signInAndSignUpPage;
    private ResetPasswordPage _resetPasswordPage;
    private EditAccountPage _editAccountPage;
    private static string indexUrl = "https://localhost:7070/";

    private ILocator _signInErrorAlert;
    private ILocator _signUpErrorAlert;
    private ILocator _resultModal;
    private ILocator _resultModalCloseButton;

    public AccountTestHelperProcedures(PlaywrightTest playwright, IPage page, SignInAndSignUpPage signInAndSignUpPage, ResetPasswordPage resetPasswordPage, EditAccountPage editAccountPage)
    {
        _playWright = playwright;
        _page = page;
        _signInAndSignUpPage = signInAndSignUpPage;
        _resetPasswordPage = resetPasswordPage;
        _editAccountPage = editAccountPage;

        _signInErrorAlert = _page.GetByTestId("signInErrorAlert");
        _signUpErrorAlert = _page.GetByTestId("signUpErrorAlert");
        _resultModal = _page.GetByTestId("resultModal");
        _resultModalCloseButton = _page.GetByTestId("resultModalCloseButton");
    }

    public async Task SignUpUser(string email = "kinnaskonstantinos0@gmail.com", string phoneNumber = "6943655624", string password = "Kinas2016!",
        string repeatPassword = "Kinas2016!", bool isOnSignUpSide = false, bool hasClientError = false, string serverError = "")
    {
        //go to the sign up page
        await _signInAndSignUpPage.NavigateToPage();
        Assert.IsTrue(await _signInAndSignUpPage.IsPageShown());

        await _signInAndSignUpPage.SubmitSignUpForm(email, phoneNumber, password, repeatPassword, isOnSignUpSide, hasClientError);

        if (hasClientError)
            return;

        if (serverError != "")
        {
            await _playWright.Expect(_signUpErrorAlert).ToBeVisibleAsync();
            await _playWright.Expect(_signUpErrorAlert).ToHaveAttributeAsync("popUpValue", serverError);
            return;
        }

        // expect to get to the confirmation page
        await _playWright.Expect(_page).ToHaveURLAsync("https://localhost:7070/Account/RegisterVerificationEmailMessage");

        string? lastEmailLink = await CommonMethods.ReadLastEmailLinkWithRetries();
        if (lastEmailLink is null)
            throw new AssertionException("The email could not be read.");

        await _page.GotoAsync(lastEmailLink);

        await _playWright.Expect(_page).ToHaveURLAsync(indexUrl);
        //here do a check best on profil
    }

    public async Task SignInUser(string email = "kinnaskonstantinos0@gmail.com", string password = "Kinas2016!",
        bool isOnSignInSide = false, bool hasClientError = false, string serverError = "")
    {
        await _signInAndSignUpPage.NavigateToPage();
        Assert.IsTrue(await _signInAndSignUpPage.IsPageShown());

        await _signInAndSignUpPage.SubmitSignInForm(email, password, isOnSignInSide, hasClientError);

        if (hasClientError)
            return;

        if (serverError != "")
        {
            await _playWright.Expect(_signInErrorAlert).ToBeVisibleAsync();
            await _playWright.Expect(_signInErrorAlert).ToHaveAttributeAsync("popUpValue", serverError);
            return;
        }

        await _playWright.Expect(_page).ToHaveURLAsync(indexUrl);
    }

    public async Task ResetUserPassword(string email = "kinnaskonstantinos0@gmail.com", string password = "Kinas2016!",
        string repeatPassword = "Kinas2016!", bool hasClientError = false, string serverError = "")
    {
        await _signInAndSignUpPage.NavigateToPage();
        Assert.IsTrue(await _signInAndSignUpPage.IsPageShown());

        await _signInAndSignUpPage.ForgotPassword(email, isOnSignInSide: true, hasClientError);

        if (hasClientError)
            return;

        if (serverError != "")
        {
            await _playWright.Expect(_signInErrorAlert).ToBeVisibleAsync();
            await _playWright.Expect(_signInErrorAlert).ToHaveAttributeAsync("popUpValue", serverError);
            return;
        }

        await _playWright.Expect(_page).ToHaveURLAsync("https://localhost:7070/Account/ResetPasswordVerificationEmailMessage");

        string? lastEmailLink = await CommonMethods.ReadLastEmailLinkWithRetries();
        if (lastEmailLink is null)
            throw new AssertionException("The email could not be read.");

        await _page.GotoAsync(lastEmailLink);

        Assert.IsTrue(await _resetPasswordPage.IsPageShown());

        await _resetPasswordPage.SubmitResetPasswordForm(password, repeatPassword, hasClientError);
        await _playWright.Expect(_page).ToHaveURLAsync(indexUrl);
    }

    public async Task ChangeUserAccountBasicSettings(string firstName = "", string lastName = "", string phoneNumber = "",
        bool hasClientError = false, string serverError = "")
    {
        await _editAccountPage.NavigateToPage();
        Assert.IsTrue(await _editAccountPage.IsPageShown());

        await _editAccountPage.ChangeAccountBasicSettings(firstName, lastName, phoneNumber, hasClientError);

        if (hasClientError)
            return;

        if (serverError != "")
        {
            await _playWright.Expect(_resultModal).ToBeVisibleAsync();
            await _playWright.Expect(_resultModal).ToHaveAttributeAsync("popUpValue", serverError);
            return;
        }

        await _playWright.Expect(_resultModal).ToBeVisibleAsync();
        await _playWright.Expect(_resultModal).ToHaveAttributeAsync("popUpValue", "accountBasicSettingsChangeSuccess");
        await _resultModalCloseButton.ClickAsync();
    }

    public async Task ChangeUserAccountPassword(string oldPassword = "Kinas2016!", string newPassword = "Kinas2020!",
        string confirmNewPassword = "Kinas2020!", bool hasClientError = false, string serverError = "")
    {
        await _editAccountPage.NavigateToPage();
        Assert.IsTrue(await _editAccountPage.IsPageShown());

        await _editAccountPage.ChangeAccountPassword(oldPassword, newPassword, confirmNewPassword, hasClientError);

        if (hasClientError)
            return;

        if (serverError != "")
        {
            await _playWright.Expect(_resultModal).ToBeVisibleAsync();
            await _playWright.Expect(_resultModal).ToHaveAttributeAsync("popUpValue", serverError);
            return;
        }

        await _playWright.Expect(_resultModal).ToBeVisibleAsync();
        await _playWright.Expect(_resultModal).ToHaveAttributeAsync("popUpValue", "passwordChangeSuccess");
        await _resultModalCloseButton.ClickAsync();
    }

    public async Task ChangeUserAccountEmail(string newEmail = "realag58@gmail.com", bool hasClientError = false, string serverError = "")
    {
        await _editAccountPage.NavigateToPage();
        Assert.IsTrue(await _editAccountPage.IsPageShown());

        await _editAccountPage.ChangeAccountEmail(newEmail, hasClientError);

        if (hasClientError)
            return;

        if (serverError != "")
        {
            await _playWright.Expect(_resultModal).ToBeVisibleAsync();
            await _playWright.Expect(_resultModal).ToHaveAttributeAsync("popUpValue", serverError);
            return;
        }

        // expect to get to the confirmation page
        await _playWright.Expect(_page).ToHaveURLAsync("https://localhost:7070/Account/ChangeEmailVerificationEmailMessage");

        string? lastEmailLink = await CommonMethods.ReadLastEmailLinkWithRetries();
        if (lastEmailLink is null)
            throw new AssertionException("The email could not be read.");

        await _page.GotoAsync(lastEmailLink);
        await _playWright.Expect(_page).ToHaveURLAsync(indexUrl);
    }

    public async Task DeleteUserAccountEmail(string email = "kinnaskonstantinos0@gmail.com", bool hasClientError = false, string serverError = "")
    {
        await _editAccountPage.NavigateToPage();
        Assert.IsTrue(await _editAccountPage.IsPageShown());

        await _editAccountPage.DeleteAccount(email, hasClientError);

        if (hasClientError)
            return;

        if (serverError != "")
        {
            await _playWright.Expect(_resultModal).ToBeVisibleAsync();
            await _playWright.Expect(_resultModal).ToHaveAttributeAsync("popUpValue", serverError);
            return;
        }

        // expect to get to the confirmation page
        await _playWright.Expect(_page).ToHaveURLAsync("https://localhost:7070/Account/DeleteAccountVerificationEmailMessage");

        string? lastEmailLink = await CommonMethods.ReadLastEmailLinkWithRetries();
        if (lastEmailLink is null)
            throw new AssertionException("The email could not be read.");
    }
}
