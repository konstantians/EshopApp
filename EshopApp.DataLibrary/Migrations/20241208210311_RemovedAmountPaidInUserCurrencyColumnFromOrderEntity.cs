﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAmountPaidInUserCurrencyColumnFromOrderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaidInCustomerCurrency",
                table: "PaymentDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaidInCustomerCurrency",
                table: "PaymentDetails",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
