using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadOptimizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeadSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: true),
                    WebhookSecret = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeadCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LeadSourceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UtmSource = table.Column<string>(type: "text", nullable: true),
                    UtmMedium = table.Column<string>(type: "text", nullable: true),
                    UtmCampaign = table.Column<string>(type: "text", nullable: true),
                    VehicleFocus = table.Column<string>(type: "text", nullable: true),
                    DefaultSalesUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadCampaigns_LeadSources_LeadSourceId",
                        column: x => x.LeadSourceId,
                        principalTable: "LeadSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadOptimizerLeads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LeadSourceId = table.Column<int>(type: "integer", nullable: true),
                    LeadCampaignId = table.Column<int>(type: "integer", nullable: true),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    ExternalLeadId = table.Column<string>(type: "text", nullable: true),
                    CustomerName = table.Column<string>(type: "text", nullable: true),
                    CustomerFirstName = table.Column<string>(type: "text", nullable: true),
                    CustomerLastName = table.Column<string>(type: "text", nullable: true),
                    CustomerEmail = table.Column<string>(type: "text", nullable: true),
                    CustomerPhone = table.Column<string>(type: "text", nullable: true),
                    VehicleMake = table.Column<string>(type: "text", nullable: true),
                    VehicleModel = table.Column<string>(type: "text", nullable: true),
                    VehicleTitle = table.Column<string>(type: "text", nullable: true),
                    VehicleUrl = table.Column<string>(type: "text", nullable: true),
                    VehiclePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    OriginalMessage = table.Column<string>(type: "text", nullable: true),
                    UtmSource = table.Column<string>(type: "text", nullable: true),
                    UtmMedium = table.Column<string>(type: "text", nullable: true),
                    UtmCampaign = table.Column<string>(type: "text", nullable: true),
                    ReferrerUrl = table.Column<string>(type: "text", nullable: true),
                    LandingPageUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    QualificationToken = table.Column<string>(type: "text", nullable: false),
                    QualificationEmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualificationOpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ScoreLabel = table.Column<string>(type: "text", nullable: false),
                    AssignedSalesUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadOptimizerLeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadOptimizerLeads_LeadCampaigns_LeadCampaignId",
                        column: x => x.LeadCampaignId,
                        principalTable: "LeadCampaigns",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeadOptimizerLeads_LeadSources_LeadSourceId",
                        column: x => x.LeadSourceId,
                        principalTable: "LeadSources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeadCampaigns_LeadSourceId",
                table: "LeadCampaigns",
                column: "LeadSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadOptimizerLeads_LeadCampaignId",
                table: "LeadOptimizerLeads",
                column: "LeadCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadOptimizerLeads_LeadSourceId",
                table: "LeadOptimizerLeads",
                column: "LeadSourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeadOptimizerLeads");

            migrationBuilder.DropTable(
                name: "LeadCampaigns");

            migrationBuilder.DropTable(
                name: "LeadSources");
        }
    }
}
