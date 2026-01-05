using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualLoginFieldsToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PictogramSequence",
                table: "PersonsWithDisability",
                newName: "EmojiSequence");

            migrationBuilder.AddColumn<string>(
                name: "AvatarColor",
                table: "PersonsWithDisability",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ColorShapeId",
                table: "PersonsWithDisability",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarColor",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "ColorShapeId",
                table: "PersonsWithDisability");

            migrationBuilder.RenameColumn(
                name: "EmojiSequence",
                table: "PersonsWithDisability",
                newName: "PictogramSequence");
        }
    }
}
