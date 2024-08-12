using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EshopApp.AuthLibrary.Models;
using Microsoft.AspNetCore.Identity;

namespace EshopApp.AuthLibrary;

public class AppIdentityDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    private readonly IConfiguration? _configuration;

    //used for migrations
    public AppIdentityDbContext()
    {

    }

    //used when the application is running
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //if the application runs use this
        if (_configuration != null)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultAuthentication"),
                options => options.EnableRetryOnFailure());
        }
        //otherwise this is used for migrations, because the configuration can not be instantiated without the application running
        else
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppAuthDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                options => options.EnableRetryOnFailure());
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        string managerRoleGuid = Guid.NewGuid().ToString();
        string adminRoleGuid = Guid.NewGuid().ToString();
        builder.Entity<AppRole>().HasData(
            new AppRole { Id = Guid.NewGuid().ToString(), Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() },
            new AppRole { Id = managerRoleGuid, Name = "Manager", NormalizedName = "MANAGER", ConcurrencyStamp = Guid.NewGuid().ToString() },
            new AppRole { Id = adminRoleGuid, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString() }
        );

        builder.Entity<AppUser>().HasData(
            new AppUser() { Id=Guid.NewGuid().ToString(), UserName = "admin@hotmail.com", Email = "admin@hotmail.com", NormalizedUserName = "ADMIN@HOTMAIL.COM", 
                NormalizedEmail = "ADMIN@HOTMAIL.COM", EmailConfirmed = true, PasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, "0XfN725l5EwSTIk!"), SecurityStamp = Guid.NewGuid().ToString()}
        );

        builder.Entity<IdentityRoleClaim<string>>().HasData(
            new IdentityRoleClaim<string> { Id = 1, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewUsers" },
            new IdentityRoleClaim<string> { Id = 2, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanEditUsers" },
            new IdentityRoleClaim<string> { Id = 3, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewAdmins" },
            new IdentityRoleClaim<string> { Id = 4, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanEditAdmins" },
            new IdentityRoleClaim<string> { Id = 5, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewUserRoles" },
            new IdentityRoleClaim<string> { Id = 6, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanEditUserRoles" },
            new IdentityRoleClaim<string> { Id = 7, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewAdminRoles" },
            new IdentityRoleClaim<string> { Id = 8, RoleId = adminRoleGuid, ClaimType = "Permission", ClaimValue = "CanEditAdminRoles" }
        );

        builder.Entity<IdentityRoleClaim<string>>().HasData(
            new IdentityRoleClaim<string> { Id = 9, RoleId = managerRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewUsers" },
            new IdentityRoleClaim<string> { Id = 10, RoleId = managerRoleGuid, ClaimType = "Permission", ClaimValue = "CanEditUsers" },
            new IdentityRoleClaim<string> { Id = 11, RoleId = managerRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewAdmins" },
            new IdentityRoleClaim<string> { Id = 12, RoleId = managerRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewUserRoles" },
            new IdentityRoleClaim<string> { Id = 13, RoleId = managerRoleGuid, ClaimType = "Permission", ClaimValue = "CanEditUserRoles" },
            new IdentityRoleClaim<string> { Id = 14, RoleId = managerRoleGuid, ClaimType = "Permission", ClaimValue = "CanViewAdminRoles" }
        );
    }
}