using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInitialCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "RequiresEmail" },
                values: new object[] { "Login visual con nombre de usuario y contrasena", false });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Login con nombre de usuario y PIN de 4 digitos");

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login seleccionando 4 emojis en orden - DEPRECADO", "Secuencia de Emojis (Deprecado)" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login seleccionando 4 colores y formas en orden - DEPRECADO", "Colores y Formas (Deprecado)" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Code", "Description", "DisplayOrder", "Name" },
                values: new object[] { "ASSISTED", "Login asistido donde un familiar o profesional autoriza el acceso", 3, "Login Asistido" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login automatico en dispositivos previamente autorizados - DEPRECADO", "Dispositivo Confiable (Deprecado)" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login seleccionando nombre y avatar del usuario - DEPRECADO", "Seleccion de Perfil (Deprecado)" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "RequiresEmail" },
                values: new object[] { "Login tradicional con email y contrasena", true });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Login con nombre de usuario y PIN de 4-6 digitos");

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login seleccionando 4 emojis en orden", "Secuencia de Emojis" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login seleccionando 4 colores y formas en orden", "Colores y Formas" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Code", "Description", "DisplayOrder", "Name" },
                values: new object[] { "SUPERVISED", "Login requiere desbloqueo por familiar o profesional", 5, "Supervisado" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login automatico en dispositivos previamente autorizados", "Dispositivo Confiable" });

            migrationBuilder.UpdateData(
                table: "LoginMethods",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Login seleccionando nombre y avatar del usuario", "Seleccion de Perfil" });
        }
    }
}
