using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opengate.Migrations
{
    /// <inheritdoc />
    public partial class UserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                            table: "AspNetUsers",
                            columns: ["Id", "FirstName", "LastName", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName", "OrganizationId"],
                            values:
                            [
                                "ca2d1747-2fb4-470b-b072-dedefa06832f", // Id
                    "Test", // FirstName
                    "User", // LastName
                    0, // AccessFailedCount
                    "5164eff0-9d4a-4953-bd04-161816497495", // ConcurrencyStamp
                    "test@test.com", // Email
                    true, // EmailConfirmed
                    true, // LockoutEnabled
                    null, // LockoutEnd
                    "TEST@TEST.COM", // NormalizedEmail
                    "TEST@TEST.COM", // NormalizedUserName
                    "AQAAAAIAAYagAAAAEMHsC/uYvh0fXRr2nS5ZQTZnzRyl1vcW0X8wu/8IR3BD8yFKgV2wpT4DPQOSbUW8jQ==", // PasswordHash
                    null, // PhoneNumber
                    false, // PhoneNumberConfirmed
                    "GX7HF7VW4G72ZQ63PIBXJDQTUQKZHQQ2", // SecurityStamp
                    false, // TwoFactorEnabled
                    "test@test.com", // UserName
                    "b7f5a8f0-8c2e-4f23-9d5c-0a4f1c7c2d9a" // OrganizationId
                            ]
                        );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                           table: "AspNetUsers",
                           keyColumn: "Id",
                           keyValue: "ca2d1747-2fb4-470b-b072-dedefa06832f"
                       );

        }
    }
}