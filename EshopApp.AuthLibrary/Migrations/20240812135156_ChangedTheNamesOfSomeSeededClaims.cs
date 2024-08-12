using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EshopApp.AuthLibrary.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTheNamesOfSomeSeededClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewUsers", "df308491-a530-404b-be4e-43ca15afda90" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanEditUsers", "df308491-a530-404b-be4e-43ca15afda90" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewAdmins", "df308491-a530-404b-be4e-43ca15afda90" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanEditAdmins", "df308491-a530-404b-be4e-43ca15afda90" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5,
                column: "RoleId",
                value: "df308491-a530-404b-be4e-43ca15afda90");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                column: "RoleId",
                value: "df308491-a530-404b-be4e-43ca15afda90");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                column: "RoleId",
                value: "df308491-a530-404b-be4e-43ca15afda90");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                column: "RoleId",
                value: "df308491-a530-404b-be4e-43ca15afda90");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewUsers", "7d7618e1-5b9a-4639-b433-05427cdde79f" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanEditUsers", "7d7618e1-5b9a-4639-b433-05427cdde79f" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewAdmins", "7d7618e1-5b9a-4639-b433-05427cdde79f" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                column: "RoleId",
                value: "7d7618e1-5b9a-4639-b433-05427cdde79f");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                column: "RoleId",
                value: "7d7618e1-5b9a-4639-b433-05427cdde79f");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                column: "RoleId",
                value: "7d7618e1-5b9a-4639-b433-05427cdde79f");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "7d7618e1-5b9a-4639-b433-05427cdde79f", "4872c74a-5c10-476a-954f-40f54a061e8b", "Manager", "MANAGER" },
                    { "cfa9cc7f-c5fa-494a-a812-f843b51c9ee2", "dacbc9bb-3f41-47cd-9bfa-c5779d37e0a3", "User", "USER" },
                    { "df308491-a530-404b-be4e-43ca15afda90", "97e547c5-2947-4390-a9aa-949dbf16d3bd", "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "47aefaf2-7592-4947-88d1-f6b424e978e0", 0, "c153c671-dc74-403c-9917-900551780536", "admin@hotmail.com", true, false, null, "ADMIN@HOTMAIL.COM", "ADMIN@HOTMAIL.COM", "AQAAAAIAAYagAAAAEGJt9WiGcXYW5kN71PUXli557wKiNAy5+ld9thr06psFvYfv3Xdxtywq8cs4q8BhsA==", null, false, "df586591-460a-453b-9b2e-5ae5702caf12", false, "admin@hotmail.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7d7618e1-5b9a-4639-b433-05427cdde79f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cfa9cc7f-c5fa-494a-a812-f843b51c9ee2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "df308491-a530-404b-be4e-43ca15afda90");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "47aefaf2-7592-4947-88d1-f6b424e978e0");

            migrationBuilder.CreateTable(
                name: "IdentityRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityRole", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewUser", "66448807-40c5-4067-b60b-a0d9d48be760" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanEditUser", "66448807-40c5-4067-b60b-a0d9d48be760" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewAdmin", "66448807-40c5-4067-b60b-a0d9d48be760" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanEditAdmin", "66448807-40c5-4067-b60b-a0d9d48be760" });

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
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewUser", "83eb3f59-e27a-4ef5-ab46-10fe52772ffe" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanEditUser", "83eb3f59-e27a-4ef5-ab46-10fe52772ffe" });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "CanViewAdmin", "83eb3f59-e27a-4ef5-ab46-10fe52772ffe" });

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
    }
}
