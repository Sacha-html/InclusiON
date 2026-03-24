using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonSkillProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonSkillProfiles",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillAreaId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonSkillProfiles", x => new { x.PersonId, x.SkillAreaId });
                    table.ForeignKey(
                        name: "FK_PersonSkillProfiles_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonSkillProfiles_SkillAreas_SkillAreaId",
                        column: x => x.SkillAreaId,
                        principalTable: "SkillAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonSkillProfiles_SkillAreaId",
                table: "PersonSkillProfiles",
                column: "SkillAreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonSkillProfiles");
        }
    }
}
