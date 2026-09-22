using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using InclusiON.Data;

#nullable disable

namespace InclusiON.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260922000000_AddAlertAcknowledgedAtToActivityAssignment")]
public partial class AddAlertAcknowledgedAtToActivityAssignment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "AlertAcknowledgedAt",
            table: "ActivityAssignments",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CustomAdaptationNotes",
            table: "ActivityAssignments",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "EstimatedDurationMinutes",
            table: "ActivityAssignments",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "HasVisualSupport",
            table: "ActivityAssignments",
            type: "boolean",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "HasAudioSupport",
            table: "ActivityAssignments",
            type: "boolean",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "UsesEasyReading",
            table: "ActivityAssignments",
            type: "boolean",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "UsesPictograms",
            table: "ActivityAssignments",
            type: "boolean",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "RequiresSupervision",
            table: "ActivityAssignments",
            type: "boolean",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AlertAcknowledgedAt",
            table: "ActivityAssignments");

        migrationBuilder.DropColumn(
            name: "CustomAdaptationNotes",
            table: "ActivityAssignments");

        migrationBuilder.DropColumn(
            name: "EstimatedDurationMinutes",
            table: "ActivityAssignments");

        migrationBuilder.DropColumn(
            name: "HasVisualSupport",
            table: "ActivityAssignments");

        migrationBuilder.DropColumn(
            name: "HasAudioSupport",
            table: "ActivityAssignments");

        migrationBuilder.DropColumn(
            name: "UsesEasyReading",
            table: "ActivityAssignments");

        migrationBuilder.DropColumn(
            name: "UsesPictograms",
            table: "ActivityAssignments");

        migrationBuilder.DropColumn(
            name: "RequiresSupervision",
            table: "ActivityAssignments");
    }
}
