using EshopApp.AuthLibrary;
using EshopApp.AuthLibrary.AuthLogic;
using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibraryAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace EshopApp.AuthLibraryAPI;

public class Program()
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IConfiguration configuration = builder.Configuration;

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; //Convoluted way of getting 'EshopApp.AuthLibraryAPI.xml', but more safe
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile); //Combine the otherpartofpath/bin/Debug/net8.0 with the EshopApp.AuthLibraryAPI.xml

            options.IncludeXmlComments(xmlPath);
        });

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

        //Probably not needed since I have learned that Cors can only be triggered with AJAX or something else
        /* List<string> excludedOrigins = new List<string>();
        if (configuration["ExcludedCorsOrigins"] is not null)
            excludedOrigins = configuration["ExcludedCorsOrigins"]!.Split(" ").ToList();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {   

                foreach(string excludedOrigin in excludedOrigins)
                {
                    builder.WithOrigins(excludedOrigin)
                       .AllowAnyHeader()
                       .AllowAnyMethod();
                }
            });
        });
        */

        builder.Services.AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultAuthentication")));

        builder.Services.Configure<IdentityOptions>(options =>
        {
            //options.Lockout.AllowedForNewUsers = true;
            //options.Lockout.MaxFailedAccessAttempts = 10;
            //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequiredLength = 8;
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };
        }).AddGoogle(options =>
        {
            options.ClientId = configuration.GetValue<string>("Authentication:Google:ClientId")!;
            options.ClientSecret = configuration.GetValue<string>("Authentication:Google:ClientSecret")!;
        }).AddTwitter(options =>
        {
            options.ConsumerKey = configuration.GetValue<string>("Authentication:Twitter:ClientId")!;
            options.ConsumerSecret = configuration.GetValue<string>("Authentication:Twitter:ClientSecret")!;
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("CanManageUsersPolicy", policy => policy.RequireClaim("Permission", "CanManageUsers"));
            options.AddPolicy("CanManageRolesPolicy", policy => policy.RequireClaim("Permission", "CanManageRoles"));
        });

        //microservice health check endpoint service
        builder.Services.AddHealthChecks()
            .AddCheck("ApiAndDatabaseHealthCheck", () => HealthCheckResult.Healthy("MicroserviceFullyOnline"))
            .AddDbContextCheck<AppIdentityDbContext>();

        builder.Services.AddScoped<IAuthenticationProcedures, AuthenticationProcedures>();
        builder.Services.AddScoped<IAdminProcedures, AdminProcedures>();
        builder.Services.AddScoped<IRoleManagementProcedures, RoleManagementProcedures>();
        builder.Services.AddScoped<IHelperMethods, HelperMethods>();

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

        app.UseHealthChecks("/api/health");

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ApiKeyProtectionMiddleware>(apiKeys);

        app.MapControllers();

        app.Run();
    }
}


