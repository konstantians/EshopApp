using EshopApp.EmailLibrary;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EshopApp.EmailLibraryAPI.HealthChecks;

public class SmtpHealthCheck : IHealthCheck
{
    private readonly IEmailService _emailService;

    public SmtpHealthCheck(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check the SMTP server
            bool smtpIsValid = _emailService.ValidateSmtpServer();
            if (smtpIsValid)
            {
                return Task.FromResult(HealthCheckResult.Healthy("SMTP server authentication successful"));
            }
            else
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("SMTP server authentication failed"));
            }
        }
        catch (Exception ex)
        {
            // Handle any exceptions and return the health check as unhealthy
            return Task.FromResult(HealthCheckResult.Unhealthy($"SMTP check failed: {ex.Message}"));
        }
    }
}
