using EshopApp.EToETests.HelperServices;

namespace EshopApp.EToETests.Tests;

[SetUpFixture]
internal class GlobalSetupFixture
{
    private IProcessManagementService _processManagementService;

    [OneTimeSetUp]
    public void OnTimeSetupUp()
    {
        _processManagementService = new ProcessManagementService();
        _processManagementService.BuildAndRunApplication();

        Thread.Sleep(4000);
    }

    [OneTimeTearDown]
    public async Task OnTimeTearDown()
    {
        _processManagementService.TerminateApplication();

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers" },
            "Auth Database Successfully Cleared!"
        );

        TestUtilitiesLibrary.DatabaseUtilities.ResetSqlDatabase(
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
            new string[] { "dbo.Attributes", "dbo.Categories", "dbo.Products", "dbo.Variants", "dbo.Discounts", "dbo.Images", "dbo.VariantImages", "dbo.Orders", "dbo.Coupons", "dbo.UserCoupons", "dbo.ShippingOptions", "dbo.PaymentOptions",
                "dbo.CartItems", "dbo.Carts" },
            "Data Database Successfully Cleared!"
        );

        await TestUtilitiesLibrary.DatabaseUtilities.ResetNoSqlDatabaseAsync(new string[] { "EshopApp_Emails" }, "All documents deleted from Email NoSql database");

        TestUtilitiesLibrary.EmailUtilities.DeleteAllEmailFiles();
    }
}
