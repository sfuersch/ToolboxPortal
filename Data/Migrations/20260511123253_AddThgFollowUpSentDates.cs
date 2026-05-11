using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddThgFollowUpSentDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUp1SentAt",
                table: "ThgCustomers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUp2SentAt",
                table: "ThgCustomers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUp3SentAt",
                table: "ThgCustomers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowUp1SentAt",
                table: "ThgCustomers");

            migrationBuilder.DropColumn(
                name: "FollowUp2SentAt",
                table: "ThgCustomers");

            migrationBuilder.DropColumn(
                name: "FollowUp3SentAt",
                table: "ThgCustomers");
        }
    }
}
