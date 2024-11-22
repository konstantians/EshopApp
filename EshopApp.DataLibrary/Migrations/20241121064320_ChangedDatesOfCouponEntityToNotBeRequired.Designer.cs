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
    [Migration("20241121064320_ChangedDatesOfCouponEntityToNotBeRequired")]
    partial class ChangedDatesOfCouponEntityToNotBeRequired
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

                    b.Property<bool?>("ExistsInOrder")
                        .IsRequired()
                        .HasColumnType("bit");

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

                    b.Property<bool?>("ExistsInOrder")
                        .IsRequired()
                        .HasColumnType("bit");

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

                    b.Property<bool?>("ExistsInOrder")
                        .IsRequired()
                        .HasColumnType("bit");

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

                    b.Property<DateTime?>("ExpirationDate")
                        .IsRequired()
                        .HasColumnType("datetime2");

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
                    b.Navigation("VariantImages");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Coupon", b =>
                {
                    b.Navigation("UserCoupons");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Discount", b =>
                {
                    b.Navigation("Variants");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Product", b =>
                {
                    b.Navigation("Variants");
                });

            modelBuilder.Entity("EshopApp.DataLibrary.Models.Variant", b =>
                {
                    b.Navigation("VariantImages");
                });
#pragma warning restore 612, 618
        }
    }
}
