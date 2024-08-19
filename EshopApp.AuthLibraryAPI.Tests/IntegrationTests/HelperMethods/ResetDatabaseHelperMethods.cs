using Microsoft.Data.SqlClient;

namespace EshopApp.AuthLibraryAPI.Tests.IntegrationTests.HelperMethods;

internal class ResetDatabaseHelperMethods
{
    public static void ResetSqlAuthDatabase()
    {

        string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        string[] tables = ["dbo.AspNetUserTokens", "dbo.AspNetUserRoles", "dbo.AspNetUserLogins", "dbo.AspNetRoles", "dbo.AspNetUsers"];
        //"dbo.AspNetUserClaims dbo.AspNetRoleClaims. This will probably be static and only changed through code"

        using SqlConnection connection = new SqlConnection(connectionString);
        connection.Open();

        string selectQuery = "SELECT Id FROM dbo.AspNetUsers WHERE Username = 'admin@hotmail.com' OR Username = 'manager@hotmail.com';";
        SqlCommand selectCommand = new SqlCommand(selectQuery, connection);
        List<string> excludedIds = new List<string>();

        using (SqlDataReader reader = selectCommand.ExecuteReader())
        {
            //janky code, but there will not be a need to add more users, so that is fine...
            while (reader.Read()) 
                excludedIds.Add(reader["Id"] is not null ? reader["Id"].ToString()! : " ");
        }
        selectCommand.ExecuteReader();
        connection.Close();

        connection.Open();

        foreach (string table in tables)
        {
            string deleteQuery;
            if (table == "dbo.AspNetRoles")
                deleteQuery = $"DELETE FROM {table} WHERE Name != 'User' AND Name != 'Admin' AND Name != 'Manager';";
            else if (table == "dbo.AspNetUsers")
                deleteQuery = $"DELETE FROM {table} WHERE Username != 'admin@hotmail.com' AND Username != 'manager@hotmail.com';";
            else if (table == "dbo.AspNetUserRoles")
            {
                deleteQuery = $"DELETE FROM {table} WHERE ";
                while (excludedIds.Count > 1)
                {
                    deleteQuery += $"UserId != '{excludedIds[0]}' AND ";
                    excludedIds.RemoveAt(0);
                }
                deleteQuery += $"UserId != '{excludedIds[0]}';";
                excludedIds.RemoveAt(0);
            }
            else
                deleteQuery = $"DELETE FROM {table};";

            using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                deleteCommand.ExecuteNonQuery();
        }

        connection.Close();

        Console.WriteLine("Auth Database Successfully Cleared!");
    }

}
