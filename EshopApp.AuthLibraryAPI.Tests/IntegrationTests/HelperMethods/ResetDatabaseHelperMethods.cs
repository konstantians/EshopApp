using Microsoft.Data.SqlClient;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;

internal class ResetDatabaseHelperMethods
{
    public static void ResetSqlAuthDatabase()
    {

        string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        string[] tables = new string[] { "dbo.AspNetRoleClaims", "dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers"}; 
        //"dbo.AspNetUserClaims. This will probably be static and only changed through code"

        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        foreach (string table in tables)
        {
            string deleteQuery;
            if(table == "dbo.AspNetRoles")
                deleteQuery = $"DELETE FROM {table} WHERE Name != 'User' AND Name != 'Admin' AND Name != 'Manager';";
            else if(table == "dbo.AspNetUsers")
                deleteQuery = $"DELETE FROM {table} WHERE Username != 'admin@hotmail.com';";
            else
                deleteQuery = $"DELETE FROM {table}";

            using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                deleteCommand.ExecuteNonQuery();
        }

        connection.Close();

        Console.WriteLine("Auth Database Successfully Cleared!");
    }

}
