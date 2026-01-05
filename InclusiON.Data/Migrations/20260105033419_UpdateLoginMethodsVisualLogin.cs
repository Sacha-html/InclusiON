using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoginMethodsVisualLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequiresPictogram",
                table: "LoginMethods",
                newName: "RequiresProfileSelect");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresColorShape",
                table: "LoginMethods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresEmojiSequence",
                table: "LoginMethods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RequiresColorShape", "RequiresEmojiSequence" },
                values: new object[] { false, false });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MinAutonomyLevel", "RequiresColorShape", "RequiresEmojiSequence" },
                values: new object[] { 1, false, false });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name", "RequiresColorShape", "RequiresEmojiSequence", "RequiresProfileSelect" },
                values: new object[] { "EMOJI_SEQUENCE", "Login seleccionando 4 emojis en orden", "Secuencia de Emojis", false, true, false });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Description", "MinAutonomyLevel", "Name", "RequiresColorShape", "RequiresEmojiSequence", "RequiresSupervisor" },
                values: new object[] { "COLOR_SHAPE", "Login seleccionando 4 colores y formas en orden", 2, "Colores y Formas", true, false, false });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Code", "Description", "Name", "RequiresColorShape", "RequiresEmojiSequence", "RequiresSupervisor" },
                values: new object[] { "SUPERVISED", "Login requiere desbloqueo por familiar o profesional", "Supervisado", false, false, true });

            migrationBuilder.InsertData(
                table: "LoginMethods",
                columns: new[] { "Id", "Code", "Description", "DisplayOrder", "IsActive", "MinAutonomyLevel", "Name", "RequiresColorShape", "RequiresEmail", "RequiresEmojiSequence", "RequiresPassword", "RequiresPin", "RequiresProfileSelect", "RequiresSupervisor" },
                values: new object[,]
                {
                    { 6, "TRUSTED_DEVICE", "Login automatico en dispositivos previamente autorizados", 6, true, 3, "Dispositivo Confiable", false, false, false, false, false, false, false },
                    { 7, "PROFILE_SELECT", "Login seleccionando nombre y avatar del usuario", 7, true, 3, "Seleccion de Perfil", false, false, false, false, false, true, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "RequiresColorShape",
                table: "LoginMethods");

            migrationBuilder.DropColumn(
                name: "RequiresEmojiSequence",
                table: "LoginMethods");

            migrationBuilder.RenameColumn(
                name: "RequiresProfileSelect",
                table: "LoginMethods",
                newName: "RequiresPictogram");

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 2,
                column: "MinAutonomyLevel",
                value: 2);

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name", "RequiresPictogram" },
                values: new object[] { "PICTOGRAM", "Login seleccionando una secuencia de imagenes", "Secuencia de Pictogramas", true });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Description", "MinAutonomyLevel", "Name", "RequiresSupervisor" },
                values: new object[] { "SUPERVISED", "Login requiere desbloqueo por familiar o profesional", 3, "Supervisado", true });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Code", "Description", "Name", "RequiresSupervisor" },
                values: new object[] { "TRUSTED_DEVICE", "Login automatico en dispositivos previamente autorizados", "Dispositivo Confiable", false });
        }
    }
}
