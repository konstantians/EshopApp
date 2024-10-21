using EshopApp.DataLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EshopApp.DataLibrary;

public class AppDataDbContext : DbContext
{
    private readonly IConfiguration? _configuration;

    //used for migrations
    public AppDataDbContext() { }

    public AppDataDbContext(DbContextOptions<AppDataDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //this is used for migrations, because the configuration can not be instantiated without the application running
        if (_configuration is not null)
        {

            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("SqlData"),
                options => options.EnableRetryOnFailure());
        }
        else
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EshopAppDataDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                options => options.EnableRetryOnFailure());
        }
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Variant> Variants { get; set; }
    public DbSet<AppAttribute> Attributes { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<VariantImage> VariantImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //product can have many variants(one to many)
        modelBuilder.Entity<Product>()
            .HasMany(product => product.Variants)
            .WithOne(variant => variant.Product)
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        //product can have many categories and a category can have many products(many to many)
        modelBuilder.Entity<Product>()
            .HasMany(product => product.Categories)
            .WithMany(categories => categories.Products);

        //variant can have many images(one to many)
        modelBuilder.Entity<Variant>()
            .HasMany(variant => variant.Images)
            .WithOne(image => image.Variant)
            .HasForeignKey(image => image.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        //variant can have many attributes and an attribute can have many variants(many to many)
        modelBuilder.Entity<Variant>()
            .HasMany(variant => variant.Attributes)
            .WithMany(attribute => attribute.Variants);

        //discounts can have many variants(one to many)
        modelBuilder.Entity<Discount>()
            .HasMany(discount => discount.Variants)
            .WithOne(variant => variant.Discount)
            .HasForeignKey(variant => variant.DiscountId);

        base.OnModelCreating(modelBuilder);
        //so what is the plan? I mean if you delete a product the variants need to go and then the variant images and then 
    }
}
