using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToLeadSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "LeadSources",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeadSources_TenantId",
                table: "LeadSources",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeadSources_Tenants_TenantId",
                table: "LeadSources",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeadSources_Tenants_TenantId",
                table: "LeadSources");

            migrationBuilder.DropIndex(
                name: "IX_LeadSources_TenantId",
                table: "LeadSources");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LeadSources");
        }
    }
}
