using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EshopApp.DataLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedSoftDeleteForProductAndVariantEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsThumbnailVariant",
                table: "Variants",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "ExistsInOrder",
                table: "Variants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeactivated",
                table: "Variants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExistsInOrder",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeactivated",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "ShouldNotShowInGallery",
                table: "Images",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ExistsInOrder",
                table: "Images",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExistsInOrder",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "IsDeactivated",
                table: "Variants");

            migrationBuilder.DropColumn(
                name: "ExistsInOrder",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeactivated",
                table: "Products");

            migrationBuilder.AlterColumn<bool>(
                name: "IsThumbnailVariant",
                table: "Variants",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ShouldNotShowInGallery",
                table: "Images",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ExistsInOrder",
                table: "Images",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }
    }
}
