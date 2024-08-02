using EshopApp.EmailLibrary.DataAccessLogic;
using EshopApp.EmailLibrary;
using Microsoft.EntityFrameworkCore;

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

        //this might not be needed since I learned how Cors work and it does not affect requests from the gateway api.
        /* builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("https://localhost:7255")
                       .AllowAnyHeader()
                       .AllowAnyMethod();

                builder.WithOrigins("https://eshopassignmentgatewayapi.azurewebsites.net/")
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });
        */

        if (configuration["DatabaseInUse"] is null || configuration["DatabaseInUse"] == "SqlServer")
        {
            builder.Services.AddDbContext<SqlEmailDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlData"))
            );

            builder.Services.AddScoped<IEmailDataAccess, SqlEmailDataAccess>();
        }
        else
        {
            builder.Services.AddCosmos<NoSqlEmailDbContext>(connectionString: configuration["CosmosDbConnectionString"]!,
                databaseName: "GlobalDb");

            builder.Services.AddScoped<IEmailDataAccess, NoSqlEmailDataAccess>();
        }

        builder.Services.AddSingleton<IEmailService, EmailService>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
