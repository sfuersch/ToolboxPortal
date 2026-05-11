using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddThgFollowUpSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThgFollowUpSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FollowUpsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    FollowUp1AfterDays = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowUp2AfterDays = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowUp3AfterDays = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxFollowUps = table.Column<int>(type: "INTEGER", nullable: false),
                    NotificationEmail = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThgFollowUpSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThgFollowUpSettings");
        }
    }
}
