using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedPaymentProcessorPaymentIntentIdColumnToPaymentProcessorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PaymentProcessorSessionId",
                table: "PaymentDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProcessorPaymentIntentId",
                table: "PaymentDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentDetails_PaymentProcessorPaymentIntentId",
                table: "PaymentDetails");

            migrationBuilder.DropIndex(
                name: "IX_PaymentDetails_PaymentProcessorSessionId",
                table: "PaymentDetails");

            migrationBuilder.DropColumn(
                name: "PaymentProcessorPaymentIntentId",
                table: "PaymentDetails");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentProcessorSessionId",
                table: "PaymentDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
