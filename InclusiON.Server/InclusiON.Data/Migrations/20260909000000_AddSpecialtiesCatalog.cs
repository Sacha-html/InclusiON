using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using InclusiON.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InclusiON.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260909000000_AddSpecialtiesCatalog")]
public partial class AddSpecialtiesCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Specialties",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            }, constraints: table => table.PrimaryKey("PK_Specialties", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Specialties_Name", table: "Specialties", column: "Name", unique: true);
        migrationBuilder.Sql("""
            INSERT INTO "Specialties" ("Id", "Name", "IsActive")
            VALUES
                (1, 'Educación Especial', TRUE),
                (2, 'Psicología', TRUE),
                (3, 'Psicopedagogía', TRUE),
                (4, 'Fonoaudiología', TRUE),
                (5, 'Terapia Ocupacional', TRUE),
                (6, 'Kinesiología', TRUE),
                (7, 'Trabajo Social', TRUE),
                (8, 'Musicoterapia', TRUE),
                (9, 'Psicomotricidad', TRUE),
                (10, 'Acompañamiento Terapéutico', TRUE),
                (11, 'Docente de Apoyo a la Inclusión (DAI)', TRUE),
                (12, 'Neurología', TRUE),
                (13, 'Pediatría', TRUE)
            ON CONFLICT DO NOTHING;
            """);

        migrationBuilder.AddColumn<int>("SpecialtyId", "Professionals", nullable: true);
        migrationBuilder.Sql("UPDATE \"Professionals\" SET \"SpecialtyId\" = CASE lower(\"Specialty\") WHEN 'educacion' THEN 1 WHEN 'educación especial' THEN 1 WHEN 'psicologia' THEN 2 WHEN 'psicología' THEN 2 WHEN 'psicopedagogia' THEN 3 WHEN 'psicopedagogía' THEN 3 WHEN 'fonoaudiologia' THEN 4 WHEN 'fonoaudiología' THEN 4 WHEN 'terapia ocupacional' THEN 5 WHEN 'kinesiologia' THEN 6 WHEN 'kinesiología' THEN 6 WHEN 'trabajo social' THEN 7 WHEN 'musicoterapia' THEN 8 WHEN 'psicomotricidad' THEN 9 WHEN 'acompañamiento terapéutico' THEN 10 WHEN 'neurologia' THEN 12 WHEN 'neurología' THEN 12 WHEN 'pediatria' THEN 13 WHEN 'pediatría' THEN 13 ELSE NULL END WHERE \"Specialty\" IS NOT NULL;");
        migrationBuilder.CreateIndex(
            name: "IX_Professionals_SpecialtyId",
            table: "Professionals",
            column: "SpecialtyId");
        migrationBuilder.AddForeignKey(
            name: "FK_Professionals_Specialties_SpecialtyId",
            table: "Professionals",
            column: "SpecialtyId",
            principalTable: "Specialties",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Professionals_Specialties_SpecialtyId",
            table: "Professionals");
        migrationBuilder.DropIndex(
            name: "IX_Professionals_SpecialtyId",
            table: "Professionals");
        migrationBuilder.DropColumn(
            name: "SpecialtyId",
            table: "Professionals");
        migrationBuilder.DropTable(
            name: "Specialties");
    }
}
