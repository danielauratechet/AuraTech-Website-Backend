using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuratechBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Organizations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Organizations",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguagePreference",
                table: "Organizations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "en-US");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Organizations",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LanguagePreference",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Organizations");
        }
    }
}
