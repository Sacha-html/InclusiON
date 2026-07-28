using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendAccessAuditResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "AccessAudits",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "AccessAudits",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "AccessAudits",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AccessAudits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessAudits_CorrelationId",
                table: "AccessAudits",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessAudits_Result",
                table: "AccessAudits",
                column: "Result");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccessAudits_CorrelationId",
                table: "AccessAudits");

            migrationBuilder.DropIndex(
                name: "IX_AccessAudits_Result",
                table: "AccessAudits");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "AccessAudits");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "AccessAudits");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AccessAudits");

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "AccessAudits",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
