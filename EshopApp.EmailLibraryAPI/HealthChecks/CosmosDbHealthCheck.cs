using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EshopApp.EmailLibraryAPI.HealthChecks;

public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public CosmosDbHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Attempt a simple operation to check the CosmosDB health
            var cosmosClient = new CosmosClient(_configuration["CosmosDbConnectionString"]);
            Database database = cosmosClient.GetDatabase(_configuration["CosmosDbDatabaseName"] ?? "GlobalDb");
            return Task.FromResult(HealthCheckResult.Healthy("Cosmos DB is reachable."));
        }
        catch (Exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Cosmos DB is not reachable."));
        }
    }
}

