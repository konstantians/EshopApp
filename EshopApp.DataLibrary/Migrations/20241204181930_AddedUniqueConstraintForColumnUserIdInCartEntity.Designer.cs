﻿// <auto-generated />
using System;
using EshopApp.DataLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    [DbContext(typeof(AppDataDbContext))]
    [Migration("20241204181930_AddedUniqueConstraintForColumnUserIdInCartEntity")]
    partial class AddedUniqueConstraintForColumnUserIdInCartEntity
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AppAttributeVariant", b =>
                {
                    b.Property<string>("AttributesId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("VariantsId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("AttributesId", "VariantsId");

                    b.HasIndex("VariantsId");

                    b.ToTable("AppAttributeVariant");
                });

            modelBuilder.Entity("CategoryProduct", b =>
                {
                    b.Property<string>("CategoriesId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProductsId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("CategoriesId", "ProductsId");

                    b.HasIndex("ProductsId");

                    b.ToTable("CategoryProduct");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.AppAttribute", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Attributes");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.AppImage", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("ExistsInOrder")
                        .HasColumnType("bit");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasMaxLength(75)
                        .HasColumnType("nvarchar(75)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool?>("ShouldNotShowInGallery")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("ImagePath")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Images");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Cart", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Carts");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.CartItem", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CartId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("Quantity")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<string>("VariantId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CartId");

                    b.HasIndex("VariantId");

                    b.ToTable("CartItems");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Category", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Coupon", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DefaultDateIntervalInDays")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DiscountPercentage")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("IsDeactivated")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<bool?>("IsUserSpecific")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TriggerEvent")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("UsageLimit")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.ToTable("Coupons");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Discount", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsDeactivated")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("Percentage")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Discounts");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CouponDiscountPercentageAtOrder")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("FinalPrice")
                        .IsRequired()
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("OrderStatus")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal?>("ShippingCostAtOrder")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ShippingOptionId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserCouponId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserId")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("ShippingOptionId");

                    b.HasIndex("UserCouponId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.OrderAddress", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AltAddress")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AltCity")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AltCountry")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AltFirstName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AltLastName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AltPhoneNumber")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("AltPostalCode")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool?>("IsShippingAddressDifferent")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("OrderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .IsUnique()
                        .HasFilter("[OrderId] IS NOT NULL");

                    b.ToTable("OrderAddresses");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.OrderItem", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DiscountId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("DiscountPercentageAtOrder")
                        .HasColumnType("int");

                    b.Property<string>("ImageId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("OrderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("Quantity")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<decimal?>("UnitPriceAtOrder")
                        .IsRequired()
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("VariantId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("DiscountId");

                    b.HasIndex("ImageId");

                    b.HasIndex("OrderId");

                    b.HasIndex("VariantId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.PaymentDetails", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<decimal?>("AmountPaidInCustomerCurrency")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("AmountPaidInEuro")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal?>("NetAmountPaidInEuro")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("OrderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PaymentCurrency")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<decimal?>("PaymentOptionExtraCostAtOrder")
                        .IsRequired()
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("PaymentOptionId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PaymentProcessorSessionId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PaymentStatus")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("OrderId")
                        .IsUnique()
                        .HasFilter("[OrderId] IS NOT NULL");

                    b.HasIndex("PaymentOptionId");

                    b.ToTable("PaymentDetails");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.PaymentOption", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("ExistsInOrder")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<decimal?>("ExtraCost")
                        .IsRequired()
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool?>("IsDeactivated")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("NameAlias")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("NameAlias")
                        .IsUnique();

                    b.ToTable("PaymentOptions");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("IsDeactivated")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Products");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.ShippingOption", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool?>("ContainsDelivery")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("ExistsInOrder")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<decimal?>("ExtraCost")
                        .IsRequired()
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool?>("IsDeactivated")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ShippingOptions");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.UserCoupon", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("CouponId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool?>("ExistsInOrder")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ExpirationDate")
                        .IsRequired()
                        .HasColumnType("datetime2");

                    b.Property<bool?>("IsDeactivated")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("StartDate")
                        .IsRequired()
                        .HasColumnType("datetime2");

                    b.Property<int?>("TimesUsed")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CouponId");

                    b.ToTable("UserCoupons");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Variant", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DiscountId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool?>("ExistsInOrder")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<bool?>("IsDeactivated")
                        .IsRequired()
                        .HasColumnType("bit");

                    b.Property<bool?>("IsThumbnailVariant")
                        .HasColumnType("bit");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("Price")
                        .IsRequired()
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SKU")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("UnitsInStock")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DiscountId");

                    b.HasIndex("ProductId");

                    b.HasIndex("SKU")
                        .IsUnique();

                    b.ToTable("Variants");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.VariantImage", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ImageId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsThumbNail")
                        .HasColumnType("bit");

                    b.Property<string>("VariantId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("VariantId");

                    b.ToTable("VariantImages");
                });

            modelBuilder.Entity("AppAttributeVariant", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.AppAttribute", null)
                        .WithMany()
                        .HasForeignKey("AttributesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EshopApp.DataLibrary.Models.Variant", null)
                        .WithMany()
                        .HasForeignKey("VariantsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CategoryProduct", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EshopApp.DataLibrary.Models.Product", null)
                        .WithMany()
                        .HasForeignKey("ProductsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.CartItem", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.Cart", "Cart")
                        .WithMany("CartItems")
                        .HasForeignKey("CartId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("EshopApp.DataLibrary.Models.Variant", "Variant")
                        .WithMany("CartItems")
                        .HasForeignKey("VariantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Cart");

                    b.Navigation("Variant");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Order", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.ShippingOption", "ShippingOption")
                        .WithMany("Orders")
                        .HasForeignKey("ShippingOptionId");

                    b.HasOne("EshopApp.DataLibrary.Models.UserCoupon", "UserCoupon")
                        .WithMany("Orders")
                        .HasForeignKey("UserCouponId");

                    b.Navigation("ShippingOption");

                    b.Navigation("UserCoupon");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.OrderAddress", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.Order", "Order")
                        .WithOne("OrderAddress")
                        .HasForeignKey("EshopApp.DataLibrary.Models.OrderAddress", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Order");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.OrderItem", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.Discount", "Discount")
                        .WithMany("OrderItems")
                        .HasForeignKey("DiscountId");

                    b.HasOne("EshopApp.DataLibrary.Models.AppImage", "Image")
                        .WithMany("OrderItems")
                        .HasForeignKey("ImageId");

                    b.HasOne("EshopApp.DataLibrary.Models.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("EshopApp.DataLibrary.Models.Variant", "Variant")
                        .WithMany("OrderItems")
                        .HasForeignKey("VariantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Discount");

                    b.Navigation("Image");

                    b.Navigation("Order");

                    b.Navigation("Variant");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.PaymentDetails", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.Order", "Order")
                        .WithOne("PaymentDetails")
                        .HasForeignKey("EshopApp.DataLibrary.Models.PaymentDetails", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("EshopApp.DataLibrary.Models.PaymentOption", "PaymentOption")
                        .WithMany("PaymentDetails")
                        .HasForeignKey("PaymentOptionId");

                    b.Navigation("Order");

                    b.Navigation("PaymentOption");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.UserCoupon", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.Coupon", "Coupon")
                        .WithMany("UserCoupons")
                        .HasForeignKey("CouponId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coupon");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Variant", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.Discount", "Discount")
                        .WithMany("Variants")
                        .HasForeignKey("DiscountId");

                    b.HasOne("EshopApp.DataLibrary.Models.Product", "Product")
                        .WithMany("Variants")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Discount");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.VariantImage", b =>
                {
                    b.HasOne("EshopApp.DataLibrary.Models.AppImage", "Image")
                        .WithMany("VariantImages")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("EshopApp.DataLibrary.Models.Variant", "Variant")
                        .WithMany("VariantImages")
                        .HasForeignKey("VariantId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Image");

                    b.Navigation("Variant");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.AppImage", b =>
                {
                    b.Navigation("OrderItems");

                    b.Navigation("VariantImages");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Cart", b =>
                {
                    b.Navigation("CartItems");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Coupon", b =>
                {
                    b.Navigation("UserCoupons");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Discount", b =>
                {
                    b.Navigation("OrderItems");

                    b.Navigation("Variants");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Order", b =>
                {
                    b.Navigation("OrderAddress");

                    b.Navigation("OrderItems");

                    b.Navigation("PaymentDetails");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.PaymentOption", b =>
                {
                    b.Navigation("PaymentDetails");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Product", b =>
                {
                    b.Navigation("Variants");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.ShippingOption", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.UserCoupon", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Variant", b =>
                {
                    b.Navigation("CartItems");

                    b.Navigation("OrderItems");

                    b.Navigation("VariantImages");
                });
#pragma warning restore 612, 618
        }
    }
}
