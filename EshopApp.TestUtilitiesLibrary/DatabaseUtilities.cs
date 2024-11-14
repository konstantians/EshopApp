using Microsoft.Data.SqlClient;

namespace EshopApp.TestUtilitiesLibrary;
public class DatabaseUtilities
{
    public static void ResetSqlAuthDatabase(string connectionString, string[] tables, string consoleMessage)
    {
        List<string> excludedIds = new List<string>();
        using SqlConnection connection = new SqlConnection(connectionString);

        if (tables.Contains("dbo.AspNetUserRoles"))
        {
            connection.Open();

            string selectQuery = "SELECT Id FROM dbo.AspNetUsers WHERE Username = 'admin@hotmail.com' OR Username = 'manager@hotmail.com';";
            SqlCommand selectCommand = new SqlCommand(selectQuery, connection);

            using (SqlDataReader reader = selectCommand.ExecuteReader())
            {
                //janky code, but what this does in simple terms is that it picks all the users that have the username given in the above select condition and
                //in the foreach loop that follows it does not allow their role-username connection(in the bridge table dbo.aspnetuserroles) to be deleted.
                while (reader.Read())
                    excludedIds.Add(reader["Id"] is not null ? reader["Id"].ToString()! : " ");
            }
            selectCommand.ExecuteReader();
            connection.Close();
        }

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

        Console.WriteLine(consoleMessage);
    }
}
