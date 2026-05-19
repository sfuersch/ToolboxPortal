using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddLeadInboundCatchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Salutation",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleConditionType",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleFirstRegistration",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleMileage",
                table: "LeadOptimizerLeads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleVin",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Zip",
                table: "LeadOptimizerLeads",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "Salutation",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "VehicleConditionType",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "VehicleFirstRegistration",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "VehicleMileage",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "VehicleVin",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "Zip",
                table: "LeadOptimizerLeads");
        }
    }
}
