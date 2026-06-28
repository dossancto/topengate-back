using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opengate.Migrations
{
    /// <inheritdoc />
    public partial class RolesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
                            table: "AspNetRoles",
                            columns: ["Id", "ConcurrencyStamp", "Name", "NormalizedName"],
                            values:
                            [
                                "8a1c4f2b-5a26-4a2b-9d77-0d6d1bb6c0a1", // Id
                                "a8fd2c5a-7082-4b80-b3fe-0db2b4d66a4b", // ConcurrencyStamp
                                "ADMIN", // Name
                                "ADMIN" // NormalizedName
                            ]
                        );

            migrationBuilder.InsertData(
                            table: "AspNetUserRoles",
                            columns: ["UserId", "RoleId"],
                            values:
                            [
                                "ca2d1747-2fb4-470b-b072-dedefa06832f", // UserId
                                "8a1c4f2b-5a26-4a2b-9d77-0d6d1bb6c0a1" // RoleId
                            ]
                        );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                           table: "AspNetUserRoles",
                           keyColumns: ["UserId", "RoleId"],
                           keyValues: ["ca2d1747-2fb4-470b-b072-dedefa06832f", "8a1c4f2b-5a26-4a2b-9d77-0d6d1bb6c0a1"]
                       );

            migrationBuilder.DeleteData(
                           table: "AspNetRoles",
                           keyColumn: "Id",
                           keyValue: "8a1c4f2b-5a26-4a2b-9d77-0d6d1bb6c0a1"
                       );
        }
    }
}