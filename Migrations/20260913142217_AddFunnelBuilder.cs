using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ToolboxPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddFunnelBuilder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Funnels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funnels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Funnels_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FunnelId = table.Column<int>(type: "integer", nullable: false),
                    Host = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainMappings_Funnels_FunnelId",
                        column: x => x.FunnelId,
                        principalTable: "Funnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FunnelSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FunnelId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    Options = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunnelSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunnelSteps_Funnels_FunnelId",
                        column: x => x.FunnelId,
                        principalTable: "Funnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FunnelId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    LastName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recipients_Funnels_FunnelId",
                        column: x => x.FunnelId,
                        principalTable: "Funnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoBindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FunnelStepId = table.Column<int>(type: "integer", nullable: false),
                    PvsPublicId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VariablesJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoBindings_FunnelSteps_FunnelStepId",
                        column: x => x.FunnelStepId,
                        principalTable: "FunnelSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FunnelEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    FunnelStepId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunnelEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunnelEvents_FunnelSteps_FunnelStepId",
                        column: x => x.FunnelStepId,
                        principalTable: "FunnelSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FunnelEvents_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FunnelLeads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    AnswersJson = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunnelLeads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FunnelLeads_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DomainMappings_FunnelId",
                table: "DomainMappings",
                column: "FunnelId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainMappings_Host",
                table: "DomainMappings",
                column: "Host",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FunnelEvents_FunnelStepId",
                table: "FunnelEvents",
                column: "FunnelStepId");

            migrationBuilder.CreateIndex(
                name: "IX_FunnelEvents_RecipientId_Type",
                table: "FunnelEvents",
                columns: new[] { "RecipientId", "Type" },
                unique: true,
                filter: "\"FunnelStepId\" IS NULL AND \"Type\" IN ('open', 'complete')");

            migrationBuilder.CreateIndex(
                name: "IX_FunnelEvents_RecipientId_Type_FunnelStepId",
                table: "FunnelEvents",
                columns: new[] { "RecipientId", "Type", "FunnelStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FunnelLeads_RecipientId",
                table: "FunnelLeads",
                column: "RecipientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funnels_Slug",
                table: "Funnels",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funnels_TenantId",
                table: "Funnels",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FunnelSteps_FunnelId_SortOrder",
                table: "FunnelSteps",
                columns: new[] { "FunnelId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_FunnelId",
                table: "Recipients",
                column: "FunnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_PublicId",
                table: "Recipients",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoBindings_FunnelStepId",
                table: "VideoBindings",
                column: "FunnelStepId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainMappings");

            migrationBuilder.DropTable(
                name: "FunnelEvents");

            migrationBuilder.DropTable(
                name: "FunnelLeads");

            migrationBuilder.DropTable(
                name: "VideoBindings");

            migrationBuilder.DropTable(
                name: "Recipients");

            migrationBuilder.DropTable(
                name: "FunnelSteps");

            migrationBuilder.DropTable(
                name: "Funnels");
        }
    }
}
