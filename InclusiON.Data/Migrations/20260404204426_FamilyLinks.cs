using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class FamilyLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyRepresentatives_Users_UserId",
                table: "FamilyRepresentatives");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndedAt",
                table: "PersonRepresentatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnlinkObservation",
                table: "PersonRepresentatives",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PersonRepresentatives",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "FamilyRepresentatives",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FamilyRepresentatives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FamilyStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyStatusHistories_FamilyRepresentatives_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "FamilyRepresentatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonRepresentativeHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonRepresentativeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepresentativeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChangeType = table.Column<int>(type: "int", nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WasPrimary = table.Column<bool>(type: "bit", nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRepresentativeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonRepresentativeHistories_FamilyRepresentatives_RepresentativeId",
                        column: x => x.RepresentativeId,
                        principalTable: "FamilyRepresentatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonRepresentativeHistories_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyStatusHistories_FamilyId",
                table: "FamilyStatusHistories",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRepresentativeHistories_PersonId",
                table: "PersonRepresentativeHistories",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRepresentativeHistories_PersonRepresentativeId",
                table: "PersonRepresentativeHistories",
                column: "PersonRepresentativeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRepresentativeHistories_RepresentativeId",
                table: "PersonRepresentativeHistories",
                column: "RepresentativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyRepresentatives_Users_UserId",
                table: "FamilyRepresentatives",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyRepresentatives_Users_UserId",
                table: "FamilyRepresentatives");

            migrationBuilder.DropTable(
                name: "FamilyStatusHistories");

            migrationBuilder.DropTable(
                name: "PersonRepresentativeHistories");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives");

            migrationBuilder.DropColumn(
                name: "EndedAt",
                table: "PersonRepresentatives");

            migrationBuilder.DropColumn(
                name: "UnlinkObservation",
                table: "PersonRepresentatives");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PersonRepresentatives");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FamilyRepresentatives");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "FamilyRepresentatives",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives",
                column: "DocumentNumber",
                unique: true,
                filter: "[DocumentNumber] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyRepresentatives_Users_UserId",
                table: "FamilyRepresentatives",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
