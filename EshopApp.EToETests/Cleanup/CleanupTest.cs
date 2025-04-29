using System.Diagnostics;

namespace EshopApp.EToETests.Cleanup;

[TestFixture]
public class CleanupTest
{
    //does nothing only used for cleanup
    //use this if you used Debug.runsettings and you want to manually cleanup processes, databases and emails
    [Test]
    public async Task DoCleanup()
    {
        var processes = Process.GetProcesses();

        // Kill useless processes
        foreach (Process process in processes)
        {
            if (process.ProcessName == "MSBuild" || process.ProcessName == "msedgewebview2" || process.ProcessName == "dotnet")
            {
                process.Kill();
            }

            if (process.ProcessName == "EshopApp.AuthLibraryApi" || process.ProcessName == "EshopApp.DataLibraryApi" ||
                process.ProcessName == "EshopApp.EmailLibraryApi" || process.ProcessName == "EshopApp.GatewayApi" || process.ProcessName == "EshopApp.MVC")
            {
                process.Kill();
            }
        }

        //reset all databases
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

        //delete all papercut emails
        TestUtilitiesLibrary.EmailUtilities.DeleteAllEmailFiles();
    }
}
