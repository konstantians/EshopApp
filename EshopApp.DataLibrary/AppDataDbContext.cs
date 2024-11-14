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

            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultData"),
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
    public DbSet<AppImage> Images { get; set; }
    public DbSet<VariantImage> VariantImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /******************* Category *******************/
        modelBuilder.Entity<Category>()
            .HasIndex(category => category.Name).IsUnique();

        modelBuilder.Entity<Category>()
            .Property(category => category.Name).HasMaxLength(50).IsRequired();

        /******************* Products *******************/
        //variant can have many variants(one to many)
        modelBuilder.Entity<Product>()
            .HasMany(product => product.Variants)
            .WithOne(variant => variant.Product)
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        //variant can have many categories and a category can have many products(many to many)
        modelBuilder.Entity<Product>()
            .HasMany(product => product.Categories)
            .WithMany(categories => categories.Products);

        modelBuilder.Entity<Product>()
            .HasIndex(product => product.Code).IsUnique();

        modelBuilder.Entity<Product>()
            .Property(product => product.Code).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Product>()
            .HasIndex(product => product.Name).IsUnique();

        modelBuilder.Entity<Product>()
            .Property(product => product.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Product>()
            .Property(product => product.IsDeactivated).IsRequired();

        modelBuilder.Entity<Product>()
            .Property(product => product.ExistsInOrder).IsRequired();

        /******************* Variants *******************/
        //variant can have many images(one to many)
        modelBuilder.Entity<Variant>()
            .HasMany(variant => variant.VariantImages)
            .WithOne(image => image.Variant)
            .HasForeignKey(image => image.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        //variant can have many attributes and an attribute can have many variants(many to many)
        modelBuilder.Entity<Variant>()
            .HasMany(variant => variant.Attributes)
            .WithMany(attribute => attribute.Variants);

        modelBuilder.Entity<Variant>()
            .HasIndex(variant => variant.SKU).IsUnique();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.SKU).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.Price).IsRequired();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.UnitsInStock).IsRequired();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.IsDeactivated).IsRequired();

        modelBuilder.Entity<Variant>()
            .Property(variant => variant.ExistsInOrder).IsRequired();

        /******************* Attributes *******************/
        modelBuilder.Entity<AppAttribute>()
            .HasIndex(attribute => attribute.Name).IsUnique();

        modelBuilder.Entity<AppAttribute>()
            .Property(attribute => attribute.Name).HasMaxLength(50).IsRequired();

        /******************* VariantImages *******************/
        modelBuilder.Entity<VariantImage>()
            .HasOne(variantImage => variantImage.Variant)
            .WithMany(variant => variant.VariantImages)
            .HasForeignKey(variantImage => variantImage.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
        //because this is a bit confusing this means that when the variant is deleted the variant images that are connected to it also will be deleted(it figures it out from which entity is the one)

        modelBuilder.Entity<VariantImage>()
            .HasOne(variantImage => variantImage.Image)
            .WithMany(image => image.VariantImages)
            .HasForeignKey(variantImage => variantImage.ImageId)
            .OnDelete(DeleteBehavior.Cascade);

        /******************* Images *******************/
        modelBuilder.Entity<AppImage>()
            .HasIndex(image => image.Name).IsUnique();

        modelBuilder.Entity<AppImage>()
            .Property(image => image.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<AppImage>()
            .HasIndex(image => image.ImagePath).IsUnique();

        modelBuilder.Entity<AppImage>()
            .Property(Image => Image.ImagePath).HasMaxLength(75).IsRequired(); //75 - 36 = 39 characters for the name of the image

        /******************* Discounts *******************/
        //discounts can have many variants(one to many)
        modelBuilder.Entity<Discount>()
            .HasMany(discount => discount.Variants)
            .WithOne(variant => variant.Discount)
            .HasForeignKey(variant => variant.DiscountId);

        modelBuilder.Entity<Discount>()
            .HasIndex(discount => discount.Name).IsUnique();

        modelBuilder.Entity<Discount>()
            .Property(discount => discount.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Discount>()
            .Property(discount => discount.Percentage).IsRequired();

        base.OnModelCreating(modelBuilder);
        //so what is the plan? I mean if you delete a variant the variants need to go and then the variant images and then 
    }
}
