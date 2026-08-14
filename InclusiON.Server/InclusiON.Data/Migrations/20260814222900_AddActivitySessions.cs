using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActivitySessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivitySessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityId = table.Column<int>(type: "integer", nullable: false),
                    DateCompleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SuccessRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false),
                    TimeSpentSeconds = table.Column<int>(type: "integer", nullable: false),
                    GasScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivitySessions_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivitySessions_PersonsWithDisability_StudentId",
                        column: x => x.StudentId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivitySessions_Professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_RoadmapOrder",
                table: "Activities",
                column: "RoadmapOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySessions_ActivityId",
                table: "ActivitySessions",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySessions_DateCompleted",
                table: "ActivitySessions",
                column: "DateCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySessions_ProfessionalId",
                table: "ActivitySessions",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySessions_StudentId",
                table: "ActivitySessions",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivitySessions");
        }
    }
}
