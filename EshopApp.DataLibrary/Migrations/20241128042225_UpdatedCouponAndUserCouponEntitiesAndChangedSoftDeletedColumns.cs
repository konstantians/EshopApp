using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCouponAndUserCouponEntitiesAndChangedSoftDeletedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExistsInOrder",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ExistsInOrder",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "ExistsInOrder",
                table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "PaymentProcessorOrderId",
                table: "PaymentDetails",
                newName: "PaymentProcessorSessionId");

            migrationBuilder.RenameColumn(
                name: "AmountPaid",
                table: "PaymentDetails",
                newName: "PaymentOptionExtraCostAtOrder");

            migrationBuilder.RenameColumn(
                name: "CouponId",
                table: "Orders",
                newName: "UserCouponId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_CouponId",
                table: "Orders",
                newName: "IX_Orders_UserCouponId");

            migrationBuilder.AddColumn<bool>(
                name: "ExistsInOrder",
                table: "UserCoupons",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeactivated",
                table: "UserCoupons",
                type: "bit",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExtraCost",
                table: "ShippingOptions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "NameAlias",
                table: "PaymentOptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaidInCustomerCurrency",
                table: "PaymentDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaidInEuro",
                table: "PaymentDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmountPaidInEuro",
                table: "PaymentDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentCurrency",
                table: "PaymentDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCostAtOrder",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOptions_NameAlias",
                table: "PaymentOptions",
                column: "NameAlias",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_UserCoupons_UserCouponId",
                table: "Orders",
                column: "UserCouponId",
                principalTable: "UserCoupons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_UserCoupons_UserCouponId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOptions_NameAlias",
                table: "PaymentOptions");

            migrationBuilder.DropColumn(
                name: "ExistsInOrder",
                table: "UserCoupons");

            migrationBuilder.DropColumn(
                name: "IsDeactivated",
                table: "UserCoupons");

            migrationBuilder.DropColumn(
                name: "NameAlias",
                table: "PaymentOptions");

            migrationBuilder.DropColumn(
                name: "AmountPaidInCustomerCurrency",
                table: "PaymentDetails");

            migrationBuilder.DropColumn(
                name: "AmountPaidInEuro",
                table: "PaymentDetails");

            migrationBuilder.DropColumn(
                name: "NetAmountPaidInEuro",
                table: "PaymentDetails");

            migrationBuilder.DropColumn(
                name: "PaymentCurrency",
                table: "PaymentDetails");

            migrationBuilder.DropColumn(
                name: "ShippingCostAtOrder",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Discounts");

            migrationBuilder.RenameColumn(
                name: "PaymentProcessorSessionId",
                table: "PaymentDetails",
                newName: "PaymentProcessorOrderId");

            migrationBuilder.RenameColumn(
                name: "PaymentOptionExtraCostAtOrder",
                table: "PaymentDetails",
                newName: "AmountPaid");

            migrationBuilder.RenameColumn(
                name: "UserCouponId",
                table: "Orders",
                newName: "CouponId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_UserCouponId",
                table: "Orders",
                newName: "IX_Orders_CouponId");

            migrationBuilder.AlterColumn<bool>(
                name: "ExtraCost",
                table: "ShippingOptions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
                name: "ExistsInOrder",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OrderItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "OrderItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ExistsInOrder",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExistsInOrder",
                table: "Coupons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id");
        }
    }
}
