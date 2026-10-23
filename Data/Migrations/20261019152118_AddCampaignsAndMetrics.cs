using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AuratechBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignsAndMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CampaignTypeId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentStep = table.Column<int>(type: "integer", nullable: false),
                    Languages = table.Column<List<string>>(type: "text[]", nullable: false),
                    AgeRanges = table.Column<List<string>>(type: "text[]", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    Interests = table.Column<List<string>>(type: "text[]", nullable: false),
                    DailyBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ScheduleType = table.Column<int>(type: "integer", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Headline = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "character varying(90)", maxLength: 90, nullable: true),
                    CallToAction = table.Column<int>(type: "integer", nullable: true),
                    CreativeImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LaunchedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Campaigns_CampaignTypes_CampaignTypeId",
                        column: x => x.CampaignTypeId,
                        principalTable: "CampaignTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Campaigns_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignDailyMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Impressions = table.Column<long>(type: "bigint", nullable: false),
                    Clicks = table.Column<long>(type: "bigint", nullable: false),
                    Spend = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignDailyMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignDailyMetrics_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignDeviceDailyMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Device = table.Column<int>(type: "integer", nullable: false),
                    Impressions = table.Column<long>(type: "bigint", nullable: false),
                    Clicks = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignDeviceDailyMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignDeviceDailyMetrics_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRegionDailyMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false),
                    Impressions = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRegionDailyMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRegionDailyMetrics_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignRegionDailyMetrics_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignRegions_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignRegions_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignDailyMetrics_CampaignId_Date",
                table: "CampaignDailyMetrics",
                columns: new[] { "CampaignId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignDailyMetrics_Date",
                table: "CampaignDailyMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignDeviceDailyMetrics_CampaignId_Date_Device",
                table: "CampaignDeviceDailyMetrics",
                columns: new[] { "CampaignId", "Date", "Device" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignDeviceDailyMetrics_Date",
                table: "CampaignDeviceDailyMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRegionDailyMetrics_CampaignId_Date_RegionId",
                table: "CampaignRegionDailyMetrics",
                columns: new[] { "CampaignId", "Date", "RegionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRegionDailyMetrics_Date",
                table: "CampaignRegionDailyMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRegionDailyMetrics_RegionId",
                table: "CampaignRegionDailyMetrics",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRegions_CampaignId_RegionId",
                table: "CampaignRegions",
                columns: new[] { "CampaignId", "RegionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignRegions_RegionId",
                table: "CampaignRegions",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CampaignTypeId",
                table: "Campaigns",
                column: "CampaignTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedByUserId",
                table: "Campaigns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId_CreatedAtUtc",
                table: "Campaigns",
                columns: new[] { "OrganizationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_OrganizationId_Status",
                table: "Campaigns",
                columns: new[] { "OrganizationId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignDailyMetrics");

            migrationBuilder.DropTable(
                name: "CampaignDeviceDailyMetrics");

            migrationBuilder.DropTable(
                name: "CampaignRegionDailyMetrics");

            migrationBuilder.DropTable(
                name: "CampaignRegions");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
