using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EshopApp.AuthLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddedFirstNameAndLastNameColumnsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5a3758d6-f7f3-4579-8ab1-45bc6cc7308c");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3011426d-9b41-4be9-8203-f15a899de7e8", "0b38f55d-c9de-49fa-8a93-8936ece154b2" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "2022b983-9e61-4752-ae67-18ae86e4db52", "b975894b-cfde-4a51-ba92-053845e4c2fa" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3011426d-9b41-4be9-8203-f15a899de7e8");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "0b38f55d-c9de-49fa-8a93-8936ece154b2");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b975894b-cfde-4a51-ba92-053845e4c2fa");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: "719c92b6-1cd2-4d09-8d10-21de29fc4ef6");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                column: "RoleId",
                value: "719c92b6-1cd2-4d09-8d10-21de29fc4ef6");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                column: "RoleId",
                value: "719c92b6-1cd2-4d09-8d10-21de29fc4ef6");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                column: "RoleId",
                value: "719c92b6-1cd2-4d09-8d10-21de29fc4ef6");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5,
                column: "RoleId",
                value: "719c92b6-1cd2-4d09-8d10-21de29fc4ef6");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                column: "RoleId",
                value: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "719c92b6-1cd2-4d09-8d10-21de29fc4ef6", "4d6e08e6-6a0d-4b64-9eca-ea509e4d5e56", "Manager", "MANAGER" },
                    { "9f3b2970-9d93-4634-bf5a-d8c801d58093", "11d24a4c-a7b3-4668-a12c-9c829328c955", "Admin", "ADMIN" },
                    { "ebea6853-a33c-4d4b-858b-f8d9bc48417f", "d4ff0d96-374f-4777-b2f2-300a5cc58fd7", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "3a3eaeb9-e7f7-407a-8e6b-906fdfadc3ac", 0, "4a0d7ffd-178a-4e6e-8c10-eef7bb19749b", "manager@hotmail.com", true, null, null, false, null, "MANAGER@HOTMAIL.COM", "MANAGER@HOTMAIL.COM", "AQAAAAIAAYagAAAAEFLCy3ZRzB9rp4eRDs4ior/5Ne6kmWiS5EEK3ctbwxKxz/rNFPaRDdsAZi3uYlC3dQ==", null, false, "572c4842-ccfa-4d1b-964d-57de2a74c0c6", false, "manager@hotmail.com" },
                    { "f731d1c6-c496-4ecd-b1ea-913988aa28c4", 0, "932349f7-9b5e-433b-bb1f-974322c6ca6d", "admin@hotmail.com", true, null, null, false, null, "ADMIN@HOTMAIL.COM", "ADMIN@HOTMAIL.COM", "AQAAAAIAAYagAAAAEOGZUxqUURf7mgLIZAFyn8XtJ8xqfSTp6xwxlmZ223I9Rof7U2umDLpwpl9ZZKZJCQ==", null, false, "00238b41-d892-49c8-9001-5eeff5e4d247", false, "admin@hotmail.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "719c92b6-1cd2-4d09-8d10-21de29fc4ef6", "3a3eaeb9-e7f7-407a-8e6b-906fdfadc3ac" },
                    { "9f3b2970-9d93-4634-bf5a-d8c801d58093", "f731d1c6-c496-4ecd-b1ea-913988aa28c4" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ebea6853-a33c-4d4b-858b-f8d9bc48417f");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "719c92b6-1cd2-4d09-8d10-21de29fc4ef6", "3a3eaeb9-e7f7-407a-8e6b-906fdfadc3ac" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "9f3b2970-9d93-4634-bf5a-d8c801d58093", "f731d1c6-c496-4ecd-b1ea-913988aa28c4" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "719c92b6-1cd2-4d09-8d10-21de29fc4ef6");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9f3b2970-9d93-4634-bf5a-d8c801d58093");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3a3eaeb9-e7f7-407a-8e6b-906fdfadc3ac");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f731d1c6-c496-4ecd-b1ea-913988aa28c4");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: "3011426d-9b41-4be9-8203-f15a899de7e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2,
                column: "RoleId",
                value: "3011426d-9b41-4be9-8203-f15a899de7e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3,
                column: "RoleId",
                value: "3011426d-9b41-4be9-8203-f15a899de7e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4,
                column: "RoleId",
                value: "3011426d-9b41-4be9-8203-f15a899de7e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5,
                column: "RoleId",
                value: "3011426d-9b41-4be9-8203-f15a899de7e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14,
                column: "RoleId",
                value: "2022b983-9e61-4752-ae67-18ae86e4db52");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2022b983-9e61-4752-ae67-18ae86e4db52", "9c0bcdc9-fd18-4aa7-84ed-80b2f6296377", "Admin", "ADMIN" },
                    { "3011426d-9b41-4be9-8203-f15a899de7e8", "378af352-90a3-47ed-9f31-fb7949c38035", "Manager", "MANAGER" },
                    { "5a3758d6-f7f3-4579-8ab1-45bc6cc7308c", "816b21f7-92e6-4e1d-a1cb-8fd74e8932b0", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "0b38f55d-c9de-49fa-8a93-8936ece154b2", 0, "c6e4af3d-1a86-4bd0-9285-b4f1085e2d7f", "manager@hotmail.com", true, false, null, "MANAGER@HOTMAIL.COM", "MANAGER@HOTMAIL.COM", "AQAAAAIAAYagAAAAEFIm6UdjYwGXjosnZRSz3FK/CgM7W7UvOC+d8mXDcim26jiKU7LcHvag+Z4ikfjTig==", null, false, "81bc8317-7da8-47e6-bf28-fd5c6c60d0e4", false, "manager@hotmail.com" },
                    { "b975894b-cfde-4a51-ba92-053845e4c2fa", 0, "930ffb38-00fd-4664-88db-7af05094147a", "admin@hotmail.com", true, false, null, "ADMIN@HOTMAIL.COM", "ADMIN@HOTMAIL.COM", "AQAAAAIAAYagAAAAEK35CcnW8rCvrl5AHfEd1TjaBf75lCjRd5yFdIVAHXDtvD+uCdSRsyq2tnCpgJFOwQ==", null, false, "b172b0fb-0c71-4470-92a8-0d0842167182", false, "admin@hotmail.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "3011426d-9b41-4be9-8203-f15a899de7e8", "0b38f55d-c9de-49fa-8a93-8936ece154b2" },
                    { "2022b983-9e61-4752-ae67-18ae86e4db52", "b975894b-cfde-4a51-ba92-053845e4c2fa" }
                });
        }
    }
}
