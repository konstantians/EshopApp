using EshopApp.DataLibrary;
using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibraryAPI.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace EshopApp.DataLibraryAPI;

public class Program()
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;
        // Add services to the container.

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;
            options.AddFixedWindowLimiter("DefaultWindowLimiter", options =>
            {
                //test value
                options.PermitLimit = 100;
                options.Window = TimeSpan.FromMinutes(1);
            });
        });

        List<string> apiKeys = new List<string>();
        if (configuration["ApiKeys"] is not null)
            apiKeys = configuration["ApiKeys"]!.Split(" ").ToList();

        builder.Services.AddDbContext<AppDataDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultData")));


        builder.Services.AddScoped<ICategoryDataAccess, CategoryDataAccess>();
        builder.Services.AddScoped<IProductDataAccess, ProductDataAccess>();
        builder.Services.AddScoped<IVariantDataAccess, VariantDataAccess>();
        builder.Services.AddScoped<IImageDataAccess, ImageDataAccess>();
        builder.Services.AddScoped<IDiscountDataAccess, DiscountDataAccess>();
        builder.Services.AddScoped<IAttributeDataAccess, AttributeDataAccess>();

        var app = builder.Build();

        // Define a condition for applying rate limiting
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

        // Configure the HTTP request pipeline.
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

