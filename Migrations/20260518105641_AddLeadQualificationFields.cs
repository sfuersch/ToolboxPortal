using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadQualificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactPreference",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchaseTimeframe",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualificationNotes",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeInVehicle",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WantsTradeIn",
                table: "LeadOptimizerLeads",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactPreference",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "PurchaseTimeframe",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "QualificationNotes",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "TradeInVehicle",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "WantsTradeIn",
                table: "LeadOptimizerLeads");
        }
    }
}
