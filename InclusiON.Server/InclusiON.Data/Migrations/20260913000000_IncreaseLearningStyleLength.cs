using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using InclusiON.Data;

#nullable disable

namespace InclusiON.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260913000000_IncreaseLearningStyleLength")]
public partial class IncreaseLearningStyleLength : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "LearningStyle",
            table: "PersonsWithDisability",
            type: "character varying(250)",
            maxLength: 250,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50,
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "LearningStyle",
            table: "PersonsWithDisability",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(250)",
            oldMaxLength: 250,
            oldNullable: true);
    }
}
