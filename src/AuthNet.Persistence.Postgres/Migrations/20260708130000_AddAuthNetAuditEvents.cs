using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthNet.Persistence.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthNetAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthNetAuditEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ActorEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TargetUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    TargetEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Metadata = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthNetAuditEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthNetAuditEvents_Action",
                table: "AuthNetAuditEvents",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuthNetAuditEvents_ActorUserId",
                table: "AuthNetAuditEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthNetAuditEvents_CreatedAtUtc",
                table: "AuthNetAuditEvents",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuthNetAuditEvents_TargetUserId",
                table: "AuthNetAuditEvents",
                column: "TargetUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthNetAuditEvents");
        }
    }
}
