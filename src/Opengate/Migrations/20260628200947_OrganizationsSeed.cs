using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Opengate.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationsSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(
                @"
insert into organizations (
  id,
  slug,
  name,
  description,
  owner_email,
  owner_phone_number,
  document,
  document_type,
  country,
  logo,
  created_at,
  updated_at,
  is_deleted,
  deleted_at
)
values (
  'b7f5a8f0-8c2e-4f23-9d5c-0a4f1c7c2d9a',
  'wellkept-demo',
  'WellKept Demo Organization',
  'Demo organization for local development.',
  'owner@wellkept.dev',
  '14155550137',
  '12345678000190',
  'CNPJ',
  'BR',
  'https://cdn.example.com/logos/wellkept.png',
  '2026-01-19 16:57:09+00',
  '2026-01-19 16:57:09+00',
  false,
  0
);
                "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DeleteData(
                          table: "organizations",
                          keyColumn: "id",
                          keyValue: "b7f5a8f0-8c2e-4f23-9d5c-0a4f1c7c2d9a"
                      );
        }
    }
}