using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class ExtendThgAutomationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRegistrationCheckEnabled",
                table: "ThgFollowUpSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BackgroundRunEveryHours",
                table: "ThgFollowUpSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxMailsPerRun",
                table: "ThgFollowUpSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRegistrationCheckEnabled",
                table: "ThgFollowUpSettings");

            migrationBuilder.DropColumn(
                name: "BackgroundRunEveryHours",
                table: "ThgFollowUpSettings");

            migrationBuilder.DropColumn(
                name: "MaxMailsPerRun",
                table: "ThgFollowUpSettings");
        }
    }
}
