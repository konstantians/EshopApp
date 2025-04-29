
namespace EshopApp.EToETests.Procedures;

internal interface IAccountTestHelperProcedures
{
    Task ChangeUserAccountBasicSettings(string firstName = "", string lastName = "", string phoneNumber = "", bool hasClientError = false, string serverError = "");
    Task ChangeUserAccountEmail(string newEmail = "realag58@gmail.com", bool hasClientError = false, string serverError = "");
    Task ChangeUserAccountPassword(string oldPassword = "Kinas2016!", string newPassword = "Kinas2020!", string confirmNewPassword = "Kinas2020!", bool hasClientError = false, string serverError = "");
    Task DeleteUserAccountEmail(string email = "kinnaskonstantinos0@gmail.com", bool hasClientError = false, string serverError = "");
    Task ResetUserPassword(string email = "kinnaskonstantinos0@gmail.com", string password = "Kinas2016!", string repeatPassword = "Kinas2016!", bool hasClientError = false, string serverError = "");
    Task SignInUser(string email = "kinnaskonstantinos0@gmail.com", string password = "Kinas2016!", bool isOnSignInSide = false, bool hasClientError = false, string serverError = "");
    Task SignUpUser(string email = "kinnaskonstantinos0@gmail.com", string phoneNumber = "6943655624", string password = "Kinas2016!", string repeatPassword = "Kinas2016!",
        bool isOnSignUpSide = false, bool hasClientError = false, string serverError = "");
}