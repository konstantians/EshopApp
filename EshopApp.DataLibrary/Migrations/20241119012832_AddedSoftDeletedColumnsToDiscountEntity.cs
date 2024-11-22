using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedSoftDeletedColumnsToDiscountEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExistsInOrder",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeactivated",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExistsInOrder",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "IsDeactivated",
                table: "Discounts");
        }
    }
}
