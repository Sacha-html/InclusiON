using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColorBlindnessTypeToPersons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorBlindnessType",
                table: "PersonsWithDisability",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorBlindnessType",
                table: "PersonsWithDisability");
        }
    }
}
