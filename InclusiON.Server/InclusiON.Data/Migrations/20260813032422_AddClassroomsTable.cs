using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassroomsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassroomId",
                table: "ProfessionalPersons",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classrooms_Professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalPersons_ClassroomId",
                table: "ProfessionalPersons",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_ProfessionalId",
                table: "Classrooms",
                column: "ProfessionalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalPersons_Classrooms_ClassroomId",
                table: "ProfessionalPersons",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalPersons_Classrooms_ClassroomId",
                table: "ProfessionalPersons");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropIndex(
                name: "IX_ProfessionalPersons_ClassroomId",
                table: "ProfessionalPersons");

            migrationBuilder.DropColumn(
                name: "ClassroomId",
                table: "ProfessionalPersons");
        }
    }
}
