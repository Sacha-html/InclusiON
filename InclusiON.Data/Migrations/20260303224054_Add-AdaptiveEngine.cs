using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdaptiveEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdaptiveAdjustmentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonRoadmapActivityId = table.Column<int>(type: "int", nullable: false),
                    ActivityResponseId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PreviousValue = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    NewValue = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AdjustedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdaptiveAdjustmentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdaptiveAdjustmentLogs_ActivityResponses_ActivityResponseId",
                        column: x => x.ActivityResponseId,
                        principalTable: "ActivityResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdaptiveAdjustmentLogs_PersonRoadmapActivities_PersonRoadmapActivityId",
                        column: x => x.PersonRoadmapActivityId,
                        principalTable: "PersonRoadmapActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdaptiveEngineConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonRoadmapActivityId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MinDifficultyLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxDifficultyLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    MinTimeLimitSeconds = table.Column<int>(type: "int", nullable: true),
                    MaxTimeLimitSeconds = table.Column<int>(type: "int", nullable: true),
                    ConsecutiveSuccessToUpgrade = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    ConsecutiveFailuresToDowngrade = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    SuccessThresholdPercent = table.Column<int>(type: "int", nullable: false, defaultValue: 70),
                    FrustrationThreshold = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdaptiveEngineConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdaptiveEngineConfigs_PersonRoadmapActivities_PersonRoadmapActivityId",
                        column: x => x.PersonRoadmapActivityId,
                        principalTable: "PersonRoadmapActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdaptiveAdjustmentLogs_ActivityResponseId",
                table: "AdaptiveAdjustmentLogs",
                column: "ActivityResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_AdaptiveAdjustmentLogs_AdjustedAt",
                table: "AdaptiveAdjustmentLogs",
                column: "AdjustedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AdaptiveAdjustmentLogs_PersonRoadmapActivityId",
                table: "AdaptiveAdjustmentLogs",
                column: "PersonRoadmapActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_AdaptiveEngineConfigs_PersonRoadmapActivityId",
                table: "AdaptiveEngineConfigs",
                column: "PersonRoadmapActivityId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdaptiveAdjustmentLogs");

            migrationBuilder.DropTable(
                name: "AdaptiveEngineConfigs");
        }
    }
}
