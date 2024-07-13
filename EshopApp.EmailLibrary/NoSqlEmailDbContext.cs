using EshopApp.EmailLibrary.Models.InternalModels.NoSqlModels;
using Microsoft.EntityFrameworkCore;

namespace EshopApp.EmailLibrary;

public class NoSqlEmailDbContext : DbContext
{
    public NoSqlEmailDbContext(DbContextOptions<NoSqlEmailDbContext> options) : base(options)
    {

    }

    internal DbSet<NoSqlEmailModel> Emails { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NoSqlEmailModel>().ToContainer("EshopApp_Emails").HasPartitionKey(email => email.Id);

        base.OnModelCreating(modelBuilder);
    }
}
