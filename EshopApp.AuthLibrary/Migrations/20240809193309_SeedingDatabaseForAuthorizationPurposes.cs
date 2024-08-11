using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EshopApp.AuthLibrary.Migrations
{
    /// <inheritdoc />
    public partial class SeedingDatabaseForAuthorizationPurposes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "29837bc8-fa01-4665-9c99-011a7eb22ab9", "77062b0f-cf46-41fd-86ff-3204d6bd97aa", "Admin", "ADMIN" },
                    { "56748679-3c9a-4b6c-8286-0b09f3d0be9d", "8b462f69-1846-4feb-8efd-1f47892371a2", "User", "USER" },
                    { "cfd9ee12-e7dd-40ce-8f83-eb77c4987554", "684ced1a-3bf9-4a59-a8c0-625b552227af", "Manager", "MANAGER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "7ceb06c1-fe10-4c72-9572-630d575eff4e", 0, "7f74b2b7-97d3-4de3-93c5-27bf2166075c", "admin@hotmail.com", true, false, null, "ADMIN@HOTMAIL.COM", "ADMIN@HOTMAIL.COM", "AQAAAAIAAYagAAAAELh+lcQXOw2KZgKUJe9u1pZP0b/icFwJ2y198gcHk226MmwZWZ07ANnSIa9GLDEyIw==", null, false, "6eeeb2bc-fa2b-49b7-9d6f-43a847a1aa11", false, "admin@hotmail.com" });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "Permission", "CanViewUser", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 2, "Permission", "CanEditUser", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 3, "Permission", "CanViewAdmin", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 4, "Permission", "CanEditAdmin", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 5, "Permission", "CanViewUserRoles", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 6, "Permission", "CanEditUserRoles", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 7, "Permission", "CanViewAdminRoles", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 8, "Permission", "CanEditAdminRoles", "29837bc8-fa01-4665-9c99-011a7eb22ab9" },
                    { 9, "Permission", "CanViewUser", "cfd9ee12-e7dd-40ce-8f83-eb77c4987554" },
                    { 10, "Permission", "CanEditUser", "cfd9ee12-e7dd-40ce-8f83-eb77c4987554" },
                    { 11, "Permission", "CanViewAdmin", "cfd9ee12-e7dd-40ce-8f83-eb77c4987554" },
                    { 12, "Permission", "CanViewUserRoles", "cfd9ee12-e7dd-40ce-8f83-eb77c4987554" },
                    { 13, "Permission", "CanEditUserRoles", "cfd9ee12-e7dd-40ce-8f83-eb77c4987554" },
                    { 14, "Permission", "CanViewAdminRoles", "cfd9ee12-e7dd-40ce-8f83-eb77c4987554" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "56748679-3c9a-4b6c-8286-0b09f3d0be9d");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7ceb06c1-fe10-4c72-9572-630d575eff4e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");
        }
    }
}
