using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadPrivacyConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrivacyAccepted",
                table: "LeadOptimizerLeads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyAcceptedAt",
                table: "LeadOptimizerLeads",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivacyAccepted",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "PrivacyAcceptedAt",
                table: "LeadOptimizerLeads");
        }
    }
}
