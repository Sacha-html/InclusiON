using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using InclusiON.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InclusiON.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261004000000_AddInstitutionalRolesCatalog")]
public partial class AddInstitutionalRolesCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "InstitutionalRoles",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InstitutionalRoles", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionalRoles_Name",
            table: "InstitutionalRoles",
            column: "Name",
            unique: true);

        migrationBuilder.Sql("""
            INSERT INTO "InstitutionalRoles" ("Id", "Name", "Description", "IsActive")
            OVERRIDING SYSTEM VALUE
            VALUES
                (1, 'Director / Directora', 'Máxima autoridad de la institución escolar o centro', TRUE),
                (2, 'Vicedirector / Vicedirectora', 'Vicedirección y gestión académica/institucional', TRUE),
                (3, 'Secretario / Secretaria', 'Administración de legajos, expedientes y documentación escolar', TRUE),
                (4, 'Preceptor / Preceptora', 'Acompañamiento, asistencia y organización de la dinámica escolar', TRUE)
            ON CONFLICT ("Name") DO NOTHING;
            """);

        migrationBuilder.AddColumn<int>(
            name: "InstitutionalRoleId",
            table: "AdminInstitutions",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_AdminInstitutions_InstitutionalRoleId",
            table: "AdminInstitutions",
            column: "InstitutionalRoleId");

        migrationBuilder.AddForeignKey(
            name: "FK_AdminInstitutions_InstitutionalRoles_InstitutionalRoleId",
            table: "AdminInstitutions",
            column: "InstitutionalRoleId",
            principalTable: "InstitutionalRoles",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AdminInstitutions_InstitutionalRoles_InstitutionalRoleId",
            table: "AdminInstitutions");

        migrationBuilder.DropIndex(
            name: "IX_AdminInstitutions_InstitutionalRoleId",
            table: "AdminInstitutions");

        migrationBuilder.DropColumn(
            name: "InstitutionalRoleId",
            table: "AdminInstitutions");

        migrationBuilder.DropTable(
            name: "InstitutionalRoles");
    }
}
