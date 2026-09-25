using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations;

public partial class SeedPuzzleActivityTemplateType : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO "ActivityTemplateTypes"
                ("SkillAreaId", "Name", "Code", "Description", "ContentSchema", "ComponentName",
                 "UsesPictograms", "HasAudio", "DisplayOrder", "CreatedAt", "CreatedBy", "IsActive")
            SELECT
                sa."Id",
                'Rompecabezas',
                'PUZZLE',
                'Rompecabezas interactivo con piezas visuales.',
                '{"type":"object","required":["instruction","pictogramId","rows","cols"]}',
                'PuzzlePlayerComponent',
                TRUE,
                FALSE,
                9,
                NOW(),
                '00000000-0000-0000-0000-000000000001',
                TRUE
            FROM "SkillAreas" sa
            WHERE sa."Name" = 'Trayectoria'
              AND NOT EXISTS (
                  SELECT 1 FROM "ActivityTemplateTypes" t WHERE t."Code" = 'PUZZLE'
              )
            ON CONFLICT ("Code") DO NOTHING;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM \"ActivityTemplateTypes\" WHERE \"Code\" = 'PUZZLE';");
    }
}
