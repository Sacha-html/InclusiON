using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedVisualLoginColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorShapeId",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "EmojiSequence",
                table: "PersonsWithDisability");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorShapeId",
                table: "PersonsWithDisability",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmojiSequence",
                table: "PersonsWithDisability",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
