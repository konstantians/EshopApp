using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedCascadeBehaviourForVariantImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VariantImages_Images_ImageId",
                table: "VariantImages");

            migrationBuilder.AddForeignKey(
                name: "FK_VariantImages_Images_ImageId",
                table: "VariantImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VariantImages_Images_ImageId",
                table: "VariantImages");

            migrationBuilder.AddForeignKey(
                name: "FK_VariantImages_Images_ImageId",
                table: "VariantImages",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }
    }
}
