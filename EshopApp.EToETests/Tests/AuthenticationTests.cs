using EshopApp.EToETests.Pages;
using EshopApp.EToETests.Procedures;

namespace EshopApp.EToETests.Tests;

[TestFixture]
internal class AuthenticationTests : PageTest
{
    private LayoutPage _layoutPage;
    private SignInAndSignUpPage _signInAndSignUpPage;
    private ResetPasswordPage _resetPasswordPage;
    private EditAccountPage _editAccountPage;
    private IAccountTestHelperProcedures _testHelperProcedures;

    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync("https://localhost:7070");
        _layoutPage = new LayoutPage(Page);
        _signInAndSignUpPage = new SignInAndSignUpPage(Page);
        _resetPasswordPage = new ResetPasswordPage(Page);
        _editAccountPage = new EditAccountPage(Page);

        _testHelperProcedures = new AccountTestHelperProcedures(this, Page, _signInAndSignUpPage, _resetPasswordPage, _editAccountPage);
    }

    [Test]
    public async Task FullAuthenticatonTest()
    {
        await _testHelperProcedures.SignUpUser(hasClientError: true);
        await _testHelperProcedures.SignUpUser(email: "admin@hotmail.com", serverError: "duplicateEmail"); //this email already exists in the database
        await _testHelperProcedures.SignUpUser(email: "kinnaskonstantinos0@gmail.com", phoneNumber: "6943655624", password: "Kinas2016!", repeatPassword: "Kinas2016!");
        await _layoutPage.LogUserOut();

        //create another user for future tests
        await _testHelperProcedures.SignUpUser(email: "another@gmail.com");
        await _layoutPage.LogUserOut();

        await _testHelperProcedures.SignInUser(isOnSignInSide: true, hasClientError: true);
        await _testHelperProcedures.SignInUser(email: "kinnaskonstantinos0@gmail.com", password: "BogusPass123@!", isOnSignInSide: true, serverError: "invalidCredentials");
        await _testHelperProcedures.SignInUser(email: "kinnaskonstantinos0@gmail.com", password: "Kinas2016!", isOnSignInSide: true);
        await _layoutPage.LogUserOut();

        await _testHelperProcedures.ResetUserPassword(hasClientError: true);
        await _testHelperProcedures.ResetUserPassword(email: "bogus@gmail.com", serverError: "userNotFoundWithGivenEmail");
        await _testHelperProcedures.ResetUserPassword(email: "kinnaskonstantinos0@gmail.com", password: "Kinas2020!", repeatPassword: "Kinas2020!");
        await _layoutPage.LogUserOut();
        await _testHelperProcedures.SignInUser(email: "kinnaskonstantinos0@gmail.com", password: "Kinas2020!", isOnSignInSide: true); //test new password

        await _testHelperProcedures.ChangeUserAccountBasicSettings(hasClientError: true);
        await _testHelperProcedures.ChangeUserAccountBasicSettings("Konstantinos", "Kinnas", "6911111111");

        await _testHelperProcedures.ChangeUserAccountPassword(hasClientError: true);
        await _testHelperProcedures.ChangeUserAccountPassword(oldPassword: "Bogus22Pass@!", newPassword: "Kinas2016!", confirmNewPassword: "Kinas2016!", serverError: "passwordMismatchError");
        await _testHelperProcedures.ChangeUserAccountPassword(oldPassword: "Kinas2020!", newPassword: "Kinas2016!", confirmNewPassword: "Kinas2016!");
        await _layoutPage.LogUserOut();
        await _testHelperProcedures.SignInUser(email: "kinnaskonstantinos0@gmail.com", password: "Kinas2016!", isOnSignInSide: true); //test new password

        await _testHelperProcedures.ChangeUserAccountEmail(hasClientError: true);
        await _testHelperProcedures.ChangeUserAccountEmail(newEmail: "admin@hotmail.com", serverError: "duplicateEmail");
        await _testHelperProcedures.ChangeUserAccountEmail(newEmail: "realag58@gmail.com");
        await _layoutPage.LogUserOut();
        await _testHelperProcedures.SignInUser(email: "realag58@gmail.com", password: "Kinas2016!", isOnSignInSide: true); //test new email

        await _testHelperProcedures.DeleteUserAccountEmail(hasClientError: true);
        await _testHelperProcedures.DeleteUserAccountEmail(email: "realag58@gmail.com");
        await _testHelperProcedures.SignInUser(email: "realag58@gmail.com", password: "Kinas2016!", isOnSignInSide: true, serverError: "userNotFoundWithGivenEmail"); //test that account has been successfully deleted
    }
}
