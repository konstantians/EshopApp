using EshopApp.EmailLibrary;
using EshopApp.EmailLibrary.DataAccessLogic;
using EshopApp.EmailLibraryAPI.HealthChecks;
using EshopApp.EmailLibraryAPI.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EshopApp.EmailLibraryAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        if (configuration["DatabaseInUse"] is null || configuration["DatabaseInUse"] == "SqlServer")
        {
            builder.Services.AddDbContext<SqlEmailDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlData"))
            );

            //microservice health check endpoint service
            builder.Services.AddHealthChecks()
                .AddCheck("ApiAndDatabaseHealthCheck", () => HealthCheckResult.Healthy("MicroserviceFullyOnline"))
                .AddDbContextCheck<SqlEmailDbContext>()
                .AddCheck<SmtpHealthCheck>("SmtpServerHealthCheck");

            builder.Services.AddScoped<IEmailDataAccess, SqlEmailDataAccess>();
        }
        else
        {
            builder.Services.AddCosmos<NoSqlEmailDbContext>(connectionString: configuration["CosmosDbConnectionString"]!,
                databaseName: builder.Configuration["CosmosDbDatabaseName"] ?? "GlobalDb");

            builder.Services.AddScoped<IEmailDataAccess, NoSqlEmailDataAccess>();

            //microservice health check endpoint service
            builder.Services.AddHealthChecks()
                .AddCheck("ApiHealthCheck", () => HealthCheckResult.Healthy("MicroserviceFullyOnline"))
                .AddCheck<CosmosDbHealthCheck>("CosmosDbHealthCheck")
                .AddCheck<SmtpHealthCheck>("SmtpServerHealthCheck");
        }

        builder.Services.AddSingleton<IEmailService, EmailService>();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddFixedWindowLimiter("DefaultWindowLimiter", options =>
            {
                options.PermitLimit = 100;
                options.Window = TimeSpan.FromMinutes(1);
            });
        });

        List<string> apiKeys = new List<string>();
        if (configuration["ApiKeys"] is not null)
            apiKeys = configuration["ApiKeys"]!.Split(" ").ToList();

        var app = builder.Build();

        // Condition for applying rate limiting
        app.UseWhen(
            context =>
            {
                context.Request.Headers.TryGetValue("X-Bypass-Rate-Limiting", out var bypassRateLimitingCode);
                if (configuration["RateLimitingBypassCode"] is null || string.IsNullOrEmpty(bypassRateLimitingCode))
                    return true;

                //if the header does not contain an accurate code for the bypassRateLimimit then use rate limiter
                return !bypassRateLimitingCode.ToString().Equals(configuration["RateLimitingBypassCode"]);
            },
            appBuilder =>
            {
                appBuilder.UseRateLimiter();
            }
        );

        app.UseHealthChecks("/api/health");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<ApiKeyProtectionMiddleware>(apiKeys);

        app.MapControllers();

        app.Run();
    }
}
