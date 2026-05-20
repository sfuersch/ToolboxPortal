using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToLeads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "LeadOptimizerLeads",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeadOptimizerLeads_TenantId",
                table: "LeadOptimizerLeads",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeadOptimizerLeads_Tenants_TenantId",
                table: "LeadOptimizerLeads",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadOptimizerLeads_Tenants_TenantId",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropIndex(
                name: "IX_LeadOptimizerLeads_TenantId",
                table: "LeadOptimizerLeads");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LeadOptimizerLeads");
        }
    }
}
