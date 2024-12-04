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
    public DbSet<AppImage> Images { get; set; }
    public DbSet<VariantImage> VariantImages { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<UserCoupon> UserCoupons { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<PaymentDetails> PaymentDetails { get; set; }
    public DbSet<PaymentOption> PaymentOptions { get; set; }
    public DbSet<ShippingOption> ShippingOptions { get; set; }
    public DbSet<OrderAddress> OrderAddresses { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

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

        modelBuilder.Entity<Discount>()
            .Property(discount => discount.IsDeactivated).IsRequired();

        base.OnModelCreating(modelBuilder);

        /******************* Coupons *******************/
        modelBuilder.Entity<Coupon>()
            .HasIndex(coupon => coupon.Code).IsUnique();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.Code).HasMaxLength(50).IsRequired(); //the code can be null since I need it in the orders even if the coupon is user specific(otherwise I could leave it null)

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.DiscountPercentage).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.UsageLimit).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.IsUserSpecific).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.IsDeactivated).IsRequired();

        modelBuilder.Entity<Coupon>()
            .Property(coupon => coupon.TriggerEvent).HasMaxLength(50).IsRequired();

        //The start and end date can be null if the event is user specific(I could give them default values, but I do not know if that makes sense)
        //The DefaultDateIntervalInDays can be null if the event is universal

        /******************* UserCoupons *******************/
        modelBuilder.Entity<UserCoupon>()
            .HasOne(userCoupon => userCoupon.Coupon)
            .WithMany(coupon => coupon.UserCoupons)
            .HasForeignKey(userCoupon => userCoupon.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.Code).HasMaxLength(50).IsRequired(); //the code of the userCoupon can be copy pasted if the coupon that creates it is universal

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.TimesUsed).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(coupon => coupon.StartDate).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(coupon => coupon.ExpirationDate).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.UserId).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.IsDeactivated).IsRequired();

        modelBuilder.Entity<UserCoupon>()
                .Property(userCoupon => userCoupon.ExistsInOrder).IsRequired();

        modelBuilder.Entity<UserCoupon>()
            .Property(userCoupon => userCoupon.CouponId).IsRequired();

        /******************* Orders *******************/
        //order can have many order items and at least one(many to one)
        modelBuilder.Entity<Order>()
            .HasMany(order => order.OrderItems)
            .WithOne(orderItem => orderItem.Order)
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        //order can have many one coupon and a coupon can have many orders(one to many)
        modelBuilder.Entity<Order>()
            .HasOne(order => order.UserCoupon)
            .WithMany(userCoupon => userCoupon.Orders)
            .HasForeignKey(order => order.UserCouponId);

        //order can have one shippingOption and a shippingOption can have many orders(one to many)
        modelBuilder.Entity<Order>()
            .HasOne(order => order.ShippingOption)
            .WithMany(shippingOption => shippingOption.Orders)
            .HasForeignKey(order => order.ShippingOptionId);

        //order can have one paymentdetails and a paymentdetails can have one order(one to one)
        modelBuilder.Entity<Order>()
            .HasOne(order => order.PaymentDetails)
            .WithOne(paymentDetails => paymentDetails.Order)
            .HasForeignKey<PaymentDetails>(paymentDetail => paymentDetail.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        //order can have one paymentdetails and a paymentdetails can have one order(one to one)
        modelBuilder.Entity<Order>()
            .HasOne(order => order.OrderAddress)
            .WithOne(orderAddress => orderAddress.Order)
            .HasForeignKey<OrderAddress>(orderAddress => orderAddress.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .Property(order => order.UserId).HasMaxLength(50); //this needs to be not required, because of the guest user

        modelBuilder.Entity<Order>()
            .Property(order => order.OrderStatus).HasMaxLength(50).IsRequired(); ////Pending - Confirmed - Processing - Shipped - Delivered - Canceled - Refunded - Failed

        modelBuilder.Entity<Order>()
            .Property(order => order.FinalPrice).IsRequired();

        /******************* OrderItems *******************/
        //orderItem can have one image and a image can have many orders(one to many)
        modelBuilder.Entity<OrderItem>()
            .HasOne(orderItem => orderItem.Image)
            .WithMany(image => image.OrderItems)
            .HasForeignKey(orderItem => orderItem.ImageId);

        //orderItem can have one variant and a variant can have many orders(one to many)
        modelBuilder.Entity<OrderItem>()
            .HasOne(orderItem => orderItem.Variant)
            .WithMany(variant => variant.OrderItems)
            .HasForeignKey(orderItem => orderItem.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        //orderItem can have one discount and a discount can have many orders(one to many)
        modelBuilder.Entity<OrderItem>()
            .HasOne(orderItem => orderItem.Discount)
            .WithMany(discount => discount.OrderItems)
            .HasForeignKey(orderItem => orderItem.DiscountId);

        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.Quantity).IsRequired();

        modelBuilder.Entity<OrderItem>()
            .Property(orderItem => orderItem.UnitPriceAtOrder).IsRequired();

        /******************* PaymentDetails *******************/
        //paymentDetails can have one paymentOption and a paymentOption can have many paymentDetails(one to many)
        modelBuilder.Entity<PaymentDetails>()
            .HasOne(paymentDetails => paymentDetails.PaymentOption)
            .WithMany(paymentOption => paymentOption.PaymentDetails)
            .HasForeignKey(paymentDetails => paymentDetails.PaymentOptionId);

        modelBuilder.Entity<PaymentDetails>()
            .Property(paymentDetails => paymentDetails.PaymentCurrency).HasMaxLength(5);

        modelBuilder.Entity<PaymentDetails>()
            .Property(paymentDetails => paymentDetails.PaymentProcessorSessionId).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<PaymentDetails>()
            .Property(paymentDetails => paymentDetails.PaymentOptionExtraCostAtOrder).IsRequired();

        modelBuilder.Entity<PaymentDetails>()
            .Property(paymentDetails => paymentDetails.PaymentStatus).HasMaxLength(50).IsRequired(); //Pending, Paid, Unpaid

        /******************* ShippingOptions *******************/
        modelBuilder.Entity<PaymentOption>()
            .HasIndex(paymentOption => paymentOption.Name).IsUnique();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<PaymentOption>()
            .HasIndex(paymentOption => paymentOption.NameAlias).IsUnique();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.NameAlias).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.ExtraCost).IsRequired();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.IsDeactivated).IsRequired();

        modelBuilder.Entity<PaymentOption>()
            .Property(paymentOption => paymentOption.ExistsInOrder).IsRequired();

        /******************* ShippingOptions *******************/
        modelBuilder.Entity<ShippingOption>()
            .HasIndex(shippingOption => shippingOption.Name).IsUnique();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.Name).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.ExtraCost).IsRequired();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.ContainsDelivery).IsRequired();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.IsDeactivated).IsRequired();

        modelBuilder.Entity<ShippingOption>()
            .Property(shippingOption => shippingOption.ExistsInOrder).IsRequired();

        /******************* OrderAddresses *******************/
        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.Email).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.FirstName).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.LastName).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.Country).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.City).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.PostalCode).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.Address).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.PhoneNumber).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.IsShippingAddressDifferent).IsRequired();

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.AltFirstName).HasMaxLength(50);

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.AltLastName).HasMaxLength(50);

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.AltCountry).HasMaxLength(50);

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.AltCity).HasMaxLength(50);

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.AltPostalCode).HasMaxLength(50);

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.AltAddress).HasMaxLength(50);

        modelBuilder.Entity<OrderAddress>()
            .Property(orderAddress => orderAddress.AltPhoneNumber).HasMaxLength(50);

        /******************* Carts *******************/
        //A cart can have many cartItems and a cartItem can have only one cart
        modelBuilder.Entity<Cart>()
            .HasMany(cart => cart.CartItems)
            .WithOne(cartItem => cartItem.Cart)
            .HasForeignKey(cartItem => cartItem.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cart>()
            .Property(cart => cart.UserId).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Cart>()
            .Property(cart => cart.CreatedAt).IsRequired();

        /******************* CartItems *******************/
        //A cart Item can have only one variant and a variant can have 0 or more cartItems
        modelBuilder.Entity<CartItem>()
            .HasOne(cartItem => cartItem.Variant)
            .WithMany(variant => variant.CartItems)
            .HasForeignKey(cartItem => cartItem.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .Property(cartItem => cartItem.Quantity).IsRequired();
    }
}
