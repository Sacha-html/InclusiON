using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyLoginMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deprecar metodos de login: EMOJI_SEQUENCE (3), COLOR_SHAPE (4), TRUSTED_DEVICE (6), PROFILE_SELECT (7)
            migrationBuilder.Sql(@"
                UPDATE LoginMethods
                SET IsActive = 0
                WHERE Id IN (3, 4, 6, 7);
            ");

            // Renombrar SUPERVISED (ID=5) a 'Login Asistido'
            migrationBuilder.Sql(@"
                UPDATE LoginMethods
                SET Name = 'Login Asistido',
                    Description = 'Login asistido donde un familiar o profesional autoriza el acceso',
                    Code = 'ASSISTED'
                WHERE Id = 5;
            ");

            // Migrar usuarios con metodos deprecados a PIN (ID=2)
            // Solo si tienen PIN configurado
            migrationBuilder.Sql(@"
                UPDATE PersonsWithDisabilities
                SET LoginMethodId = 2
                WHERE LoginMethodId IN (3, 4, 6, 7)
                  AND PinCodeHash IS NOT NULL;
            ");

            // Migrar usuarios sin PIN a ASSISTED (ID=5)
            migrationBuilder.Sql(@"
                UPDATE PersonsWithDisabilities
                SET LoginMethodId = 5
                WHERE LoginMethodId IN (3, 4, 6, 7)
                  AND PinCodeHash IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reactivar metodos de login deprecados
            migrationBuilder.Sql(@"
                UPDATE LoginMethods
                SET IsActive = 1
                WHERE Id IN (3, 4, 6, 7);
            ");

            // Restaurar nombre original de SUPERVISED
            migrationBuilder.Sql(@"
                UPDATE LoginMethods
                SET Name = 'Supervisado',
                    Description = 'Login requiere desbloqueo por familiar o profesional',
                    Code = 'SUPERVISED'
                WHERE Id = 5;
            ");

            // Nota: No se puede revertir la migracion de usuarios ya que no sabemos
            // cual era su metodo original. Los usuarios migrados permanecen con PIN o ASSISTED.
        }
    }
}
