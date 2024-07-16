using Microsoft.Azure.Cosmos;

namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.HelperMethods;

internal class ResetDatabaseHelperMethods
{
    internal static async Task ResetNoSqlEmailDatabaseAsync()
    {
        string cosmosDbConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        CosmosClient cosmosClient = new CosmosClient(cosmosDbConnectionString);
        Database database = cosmosClient.GetDatabase("GlobalDb");

        List<Container> containers = new() { database.GetContainer("EshopApp_Emails") };

        foreach (Container container in containers)
        {
            //all the documents of the container
            FeedIterator<dynamic> resultSetIterator = container.GetItemQueryIterator<dynamic>("SELECT * FROM c");

            while (resultSetIterator.HasMoreResults)
            {
                //a batch of the documents that are loaded from resultSetIterator
                FeedResponse<dynamic> response = await resultSetIterator.ReadNextAsync();

                foreach (var document in response)
                {
                    await container.DeleteItemAsync<dynamic>(id: document.id.ToString(),
                        partitionKey: new PartitionKey(document.Id.ToString()));
                }
            }
        }

        Console.WriteLine("All documents deleted from NoSql database.");
    }

}
