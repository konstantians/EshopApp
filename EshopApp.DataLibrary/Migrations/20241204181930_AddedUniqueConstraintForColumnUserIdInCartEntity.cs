using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedUniqueConstraintForColumnUserIdInCartEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId",
                table: "Carts");
        }
    }
}
