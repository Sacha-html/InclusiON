using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkillAreaId",
                table: "Activities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PersonRoadmaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRoadmaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonRoadmaps_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonRoadmaps_Professionals_CreatedByProfessionalId",
                        column: x => x.CreatedByProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SkillAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTemplateTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillAreaId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentSchema = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    ComponentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsesPictograms = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasAudio = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTemplateTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTemplateTypes_SkillAreas_SkillAreaId",
                        column: x => x.SkillAreaId,
                        principalTable: "SkillAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonRoadmapAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonRoadmapId = table.Column<int>(type: "int", nullable: false),
                    SkillAreaId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRoadmapAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonRoadmapAreas_PersonRoadmaps_PersonRoadmapId",
                        column: x => x.PersonRoadmapId,
                        principalTable: "PersonRoadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonRoadmapAreas_SkillAreas_SkillAreaId",
                        column: x => x.SkillAreaId,
                        principalTable: "SkillAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    TemplateTypeId = table.Column<int>(type: "int", nullable: false),
                    ContentJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityContents_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityContents_ActivityTemplateTypes_TemplateTypeId",
                        column: x => x.TemplateTypeId,
                        principalTable: "ActivityTemplateTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonRoadmapActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonRoadmapAreaId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    IsUnlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnlockThresholdPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 60),
                    TimeLimitSeconds = table.Column<int>(type: "int", nullable: true),
                    MaxAttempts = table.Column<int>(type: "int", nullable: true),
                    ShowHints = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRoadmapActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonRoadmapActivities_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonRoadmapActivities_PersonRoadmapAreas_PersonRoadmapAreaId",
                        column: x => x.PersonRoadmapAreaId,
                        principalTable: "PersonRoadmapAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SkillAreaId",
                table: "Activities",
                column: "SkillAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContents_ActivityId",
                table: "ActivityContents",
                column: "ActivityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContents_TemplateTypeId",
                table: "ActivityContents",
                column: "TemplateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTemplateTypes_Code",
                table: "ActivityTemplateTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTemplateTypes_SkillAreaId",
                table: "ActivityTemplateTypes",
                column: "SkillAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmapActivities_ActivityId",
                table: "PersonRoadmapActivities",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmapActivities_PersonRoadmapAreaId_ActivityId",
                table: "PersonRoadmapActivities",
                columns: new[] { "PersonRoadmapAreaId", "ActivityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmapActivities_PersonRoadmapAreaId_IsUnlocked",
                table: "PersonRoadmapActivities",
                columns: new[] { "PersonRoadmapAreaId", "IsUnlocked" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmapActivities_PersonRoadmapAreaId_SequenceOrder",
                table: "PersonRoadmapActivities",
                columns: new[] { "PersonRoadmapAreaId", "SequenceOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmapAreas_PersonRoadmapId_SkillAreaId",
                table: "PersonRoadmapAreas",
                columns: new[] { "PersonRoadmapId", "SkillAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmapAreas_SkillAreaId",
                table: "PersonRoadmapAreas",
                column: "SkillAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmaps_CreatedByProfessionalId",
                table: "PersonRoadmaps",
                column: "CreatedByProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRoadmaps_PersonId",
                table: "PersonRoadmaps",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkillAreas_DisplayOrder",
                table: "SkillAreas",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SkillAreas_Name",
                table: "SkillAreas",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_SkillAreas_SkillAreaId",
                table: "Activities",
                column: "SkillAreaId",
                principalTable: "SkillAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_SkillAreas_SkillAreaId",
                table: "Activities");

            migrationBuilder.DropTable(
                name: "ActivityContents");

            migrationBuilder.DropTable(
                name: "PersonRoadmapActivities");

            migrationBuilder.DropTable(
                name: "ActivityTemplateTypes");

            migrationBuilder.DropTable(
                name: "PersonRoadmapAreas");

            migrationBuilder.DropTable(
                name: "PersonRoadmaps");

            migrationBuilder.DropTable(
                name: "SkillAreas");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SkillAreaId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "SkillAreaId",
                table: "Activities");
        }
    }
}
