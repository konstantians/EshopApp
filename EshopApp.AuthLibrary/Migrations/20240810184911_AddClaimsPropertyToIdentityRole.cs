using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EshopApp.AuthLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimsPropertyToIdentityRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "56748679-3c9a-4b6c-8286-0b09f3d0be9d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7ceb06c1-fe10-4c72-9572-630d575eff4e");

            migrationBuilder.CreateTable(
                name: "IdentityRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                column: "RoleId",
                value: "66448807-40c5-4067-b60b-a0d9d48be760");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                column: "RoleId",
                value: "83eb3f59-e27a-4ef5-ab46-10fe52772ffe");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                column: "RoleId",
                value: "83eb3f59-e27a-4ef5-ab46-10fe52772ffe");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                column: "RoleId",
                value: "83eb3f59-e27a-4ef5-ab46-10fe52772ffe");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                column: "RoleId",
                value: "83eb3f59-e27a-4ef5-ab46-10fe52772ffe");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                column: "RoleId",
                value: "83eb3f59-e27a-4ef5-ab46-10fe52772ffe");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                column: "RoleId",
                value: "83eb3f59-e27a-4ef5-ab46-10fe52772ffe");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "bc4e7fe5-6779-4531-9938-33514b7557db", 0, "375e514b-2da8-4ec6-abc6-9cedcf8eb2b8", "admin@hotmail.com", true, false, null, "ADMIN@HOTMAIL.COM", "ADMIN@HOTMAIL.COM", "AQAAAAIAAYagAAAAEPvGdgzdbB/1BT0b4CFzkkwpNKnxPyCr0qq8BG5PL/QpQyri00lIZUUJXbORD/MjQw==", null, false, "5c8af473-7772-4deb-b338-b1b94d2a1b5a", false, "admin@hotmail.com" });

            migrationBuilder.InsertData(
                table: "IdentityRole",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "05888481-649e-46f9-96aa-2588ff5b00cb", "92824b71-f797-4b82-a279-8b2655b77677", "User", "USER" },
                    { "66448807-40c5-4067-b60b-a0d9d48be760", "5a48029f-e8b2-40db-aa4e-adec6deef27c", "Admin", "ADMIN" },
                    { "83eb3f59-e27a-4ef5-ab46-10fe52772ffe", "8f167eb5-134d-4046-85fa-f5ebfdc1f112", "Manager", "MANAGER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentityRole");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "bc4e7fe5-6779-4531-9938-33514b7557db");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                column: "RoleId",
                value: "29837bc8-fa01-4665-9c99-011a7eb22ab9");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                column: "RoleId",
                value: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                column: "RoleId",
                value: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                column: "RoleId",
                value: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                column: "RoleId",
                value: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                column: "RoleId",
                value: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                column: "RoleId",
                value: "cfd9ee12-e7dd-40ce-8f83-eb77c4987554");

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
        }
    }
}
