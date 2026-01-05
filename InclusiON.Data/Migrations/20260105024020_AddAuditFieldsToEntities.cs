using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TrustedDevices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TrustedDevices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TrustedDevices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "TrustedDevices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ActivityResponses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ActivityResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ActivityResponses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ActivityResponses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ActivityAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "ActivityAssignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ActivityAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "ActivityAssignments",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TrustedDevices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TrustedDevices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TrustedDevices");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TrustedDevices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ActivityResponses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ActivityResponses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ActivityResponses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ActivityResponses");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ActivityAssignments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ActivityAssignments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ActivityAssignments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ActivityAssignments");
        }
    }
}
