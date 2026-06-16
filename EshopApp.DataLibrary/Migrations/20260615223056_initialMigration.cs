using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class initialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attributes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryIconLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercentage = table.Column<int>(type: "int", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: false),
                    DefaultDateIntervalInDays = table.Column<int>(type: "int", nullable: true),
                    IsUserSpecific = table.Column<bool>(type: "bit", nullable: false),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    TriggerEvent = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Percentage = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    ShouldNotShowInGallery = table.Column<bool>(type: "bit", nullable: true),
                    ExistsInOrder = table.Column<bool>(type: "bit", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NameAlias = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    ExistsInOrder = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingOptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContainsDelivery = table.Column<bool>(type: "bit", nullable: false),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    ExistsInOrder = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserCoupons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimesUsed = table.Column<int>(type: "int", nullable: false),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    ExistsInOrder = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CouponId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCoupons_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryProduct",
                columns: table => new
                {
                    CategoriesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryProduct", x => new { x.CategoriesId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_CategoryProduct_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryProduct_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Variants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsThumbnailVariant = table.Column<bool>(type: "bit", nullable: true),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    ExistsInOrder = table.Column<bool>(type: "bit", nullable: false),
                    UnitsInStock = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DiscountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Variants_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Variants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingCostAtOrder = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CouponDiscountPercentageAtOrder = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShippingOptionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserCouponId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_ShippingOptions_ShippingOptionId",
                        column: x => x.ShippingOptionId,
                        principalTable: "ShippingOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_UserCoupons_UserCouponId",
                        column: x => x.UserCouponId,
                        principalTable: "UserCoupons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppAttributeVariant",
                columns: table => new
                {
                    AttributesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VariantsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAttributeVariant", x => new { x.AttributesId, x.VariantsId });
                    table.ForeignKey(
                        name: "FK_AppAttributeVariant_Attributes_AttributesId",
                        column: x => x.AttributesId,
                        principalTable: "Attributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppAttributeVariant_Variants_VariantsId",
                        column: x => x.VariantsId,
                        principalTable: "Variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CartId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VariantId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "Variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VariantImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsThumbNail = table.Column<bool>(type: "bit", nullable: false),
                    VariantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ImageId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VariantImages_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VariantImages_Variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "Variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAddresses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsShippingAddressDifferent = table.Column<bool>(type: "bit", nullable: false),
                    AltFirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AltLastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AltCountry = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AltCity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AltPostalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AltAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AltPhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderAddresses_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPriceAtOrder = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentageAtOrder = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ImageId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    VariantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DiscountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_Images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "Images",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "Variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetails",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentCurrency = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    AmountPaidInEuro = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetAmountPaidInEuro = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentOptionExtraCostAtOrder = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentProcessorSessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentProcessorPaymentIntentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentOptionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_PaymentOptions_PaymentOptionId",
                        column: x => x.PaymentOptionId,
                        principalTable: "PaymentOptions",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "PaymentOptions",
                columns: new[] { "Id", "CreatedAt", "Description", "ExistsInOrder", "ExtraCost", "IsDeactivated", "ModifiedAt", "Name", "NameAlias" },
                values: new object[,]
                {
                    { "payment_card", new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3377), "Ασφαλής πληρωμή με κάρτα Visa, Mastercard ή Maestro", false, 0.00m, false, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3378), "Πιστωτική/Χρεωστική Κάρτα", "card" },
                    { "payment_cod", new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3368), "Πληρώστε με μετρητά κατά την παράδοση της παραγγελίας σας", false, 2.00m, false, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3373), "Αντικαταβολή", "cod" },
                    { "payment_paypal", new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3381), "Πληρωμή μέσω του ασφαλούς συστήματος PayPal", false, 0.00m, false, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3383), "PayPal", "paypal" }
                });

            migrationBuilder.InsertData(
                table: "ShippingOptions",
                columns: new[] { "Id", "ContainsDelivery", "CreatedAt", "Description", "ExistsInOrder", "ExtraCost", "IsDeactivated", "ModifiedAt", "Name" },
                values: new object[,]
                {
                    { "ship_express", true, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3205), "Παράδοση σε 1–2 εργάσιμες μέρες", false, 5.00m, false, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3207), "Ταχεία Αποστολή" },
                    { "ship_pickup", false, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3210), "Παραλάβετε την παραγγελία σας από το κατάστημα μας", false, 0.00m, false, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3211), "Παραλαβή Από Το Κατάστημα" },
                    { "ship_standard", true, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3156), "Παράδοση σε 3–5 εργάσιμες μέρες", false, 3.00m, false, new DateTime(2026, 6, 16, 1, 30, 56, 464, DateTimeKind.Local).AddTicks(3201), "Κανονική Αποστολή" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAttributeVariant_VariantsId",
                table: "AppAttributeVariant",
                column: "VariantsId");

            migrationBuilder.CreateIndex(
                name: "IX_Attributes_Name",
                table: "Attributes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_VariantId",
                table: "CartItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryProduct_ProductsId",
                table: "CategoryProduct",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_Name",
                table: "Discounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_ImagePath",
                table: "Images",
                column: "ImagePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_Name",
                table: "Images",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddresses_OrderId",
                table: "OrderAddresses",
                column: "OrderId",
                unique: true,
                filter: "[OrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_DiscountId",
                table: "OrderItems",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ImageId",
                table: "OrderItems",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VariantId",
                table: "OrderItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingOptionId",
                table: "Orders",
                column: "ShippingOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserCouponId",
                table: "Orders",
                column: "UserCouponId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_OrderId",
                table: "PaymentDetails",
                column: "OrderId",
                unique: true,
                filter: "[OrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentOptionId",
                table: "PaymentDetails",
                column: "PaymentOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentProcessorPaymentIntentId",
                table: "PaymentDetails",
                column: "PaymentProcessorPaymentIntentId",
                unique: true,
                filter: "[PaymentProcessorPaymentIntentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentProcessorSessionId",
                table: "PaymentDetails",
                column: "PaymentProcessorSessionId",
                unique: true,
                filter: "[PaymentProcessorSessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOptions_Name",
                table: "PaymentOptions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOptions_NameAlias",
                table: "PaymentOptions",
                column: "NameAlias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingOptions_Name",
                table: "ShippingOptions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCoupons_CouponId",
                table: "UserCoupons",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantImages_ImageId",
                table: "VariantImages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantImages_VariantId",
                table: "VariantImages",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Variants_DiscountId",
                table: "Variants",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_Variants_ProductId",
                table: "Variants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Variants_SKU",
                table: "Variants",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAttributeVariant");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "CategoryProduct");

            migrationBuilder.DropTable(
                name: "OrderAddresses");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "PaymentDetails");

            migrationBuilder.DropTable(
                name: "VariantImages");

            migrationBuilder.DropTable(
                name: "Attributes");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PaymentOptions");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Variants");

            migrationBuilder.DropTable(
                name: "ShippingOptions");

            migrationBuilder.DropTable(
                name: "UserCoupons");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Coupons");
        }
    }
}
