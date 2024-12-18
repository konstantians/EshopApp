using Microsoft.Extensions.Diagnostics.HealthChecks;
using Stripe;

namespace EshopApp.TransactionLibraryAPI.HealthChecks;

public class StripeHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new Stripe.AccountService();
            var account = await service.GetSelfAsync(); //I have no idea what this returns, but it is an easy way to check if the stripe api and account are correctly set
            return HealthCheckResult.Healthy("Stripe API is reachable and API key is valid.");
        }
        catch (StripeException ex)
        {
            return HealthCheckResult.Unhealthy($"Stripe API is unreachable or API key is invalid: {ex.Message}");
        }
    }
}

