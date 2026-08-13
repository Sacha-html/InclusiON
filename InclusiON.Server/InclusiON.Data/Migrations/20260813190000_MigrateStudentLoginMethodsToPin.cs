using System;
using Microsoft.EntityFrameworkCore.Migrations;
using BCrypt.Net;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateStudentLoginMethodsToPin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hash por defecto para el PIN '1234'
            var defaultPinHash = BCrypt.Net.BCrypt.HashPassword("1234", BCrypt.Net.BCrypt.GenerateSalt(12));

            // Actualizar a todos los alumnos (PersonsWithDisability) que tengan LoginMethodId = 1 (Email/STANDARD) o NULL
            // para que usen LoginMethodId = 2 (PIN) y credencial por defecto '1234'
            migrationBuilder.Sql($@"
                UPDATE ""PersonsWithDisability""
                SET ""LoginMethodId"" = 2,
                    ""PinCodeHash"" = '{defaultPinHash}'
                WHERE ""LoginMethodId"" = 1 OR ""LoginMethodId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir a LoginMethodId = 1 si fuera necesario
        }
    }
}
