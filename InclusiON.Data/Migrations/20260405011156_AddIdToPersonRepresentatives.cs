using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToPersonRepresentatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonRepresentatives",
                table: "PersonRepresentatives");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PersonRepresentatives",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Relationship",
                table: "PersonRepresentatives",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonRepresentatives",
                table: "PersonRepresentatives",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRepresentatives_PersonId_RepresentativeId",
                table: "PersonRepresentatives",
                columns: new[] { "PersonId", "RepresentativeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonRepresentatives",
                table: "PersonRepresentatives");

            migrationBuilder.DropIndex(
                name: "IX_PersonRepresentatives_PersonId_RepresentativeId",
                table: "PersonRepresentatives");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PersonRepresentatives");

            migrationBuilder.DropColumn(
                name: "Relationship",
                table: "PersonRepresentatives");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonRepresentatives",
                table: "PersonRepresentatives",
                columns: new[] { "PersonId", "RepresentativeId" });
        }
    }
}
