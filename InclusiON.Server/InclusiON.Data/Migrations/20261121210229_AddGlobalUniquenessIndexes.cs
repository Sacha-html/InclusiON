using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalUniquenessIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Normalizar espacios, strings vacíos y capitalización antes de crear índices únicos
            migrationBuilder.Sql("""
                UPDATE "PersonsWithDisability"
                SET "DocumentNumber" = NULLIF(TRIM("DocumentNumber"), '')
                WHERE "DocumentNumber" IS NOT NULL
                  AND "DocumentNumber" IS DISTINCT FROM NULLIF(TRIM("DocumentNumber"), '');

                UPDATE "FamilyRepresentatives"
                SET "DocumentNumber" = NULLIF(TRIM("DocumentNumber"), '')
                WHERE "DocumentNumber" IS NOT NULL
                  AND "DocumentNumber" IS DISTINCT FROM NULLIF(TRIM("DocumentNumber"), '');

                UPDATE "Professionals"
                SET "DocumentNumber" = NULLIF(TRIM("DocumentNumber"), '')
                WHERE "DocumentNumber" IS NOT NULL
                  AND "DocumentNumber" IS DISTINCT FROM NULLIF(TRIM("DocumentNumber"), '');

                UPDATE "Professionals"
                SET "LicenseNumber" = NULLIF(TRIM("LicenseNumber"), '')
                WHERE "LicenseNumber" IS NOT NULL
                  AND "LicenseNumber" IS DISTINCT FROM NULLIF(TRIM("LicenseNumber"), '');

                UPDATE "Professionals"
                SET "Email" = NULLIF(LOWER(TRIM("Email")), '')
                WHERE "Email" IS NOT NULL
                  AND "Email" IS DISTINCT FROM NULLIF(LOWER(TRIM("Email")), '');

                UPDATE "Users"
                SET "Email" = NULLIF(LOWER(TRIM("Email")), ''),
                    "NormalizedEmail" = NULLIF(UPPER(TRIM("Email")), '')
                WHERE "Email" IS NOT NULL
                  AND (
                    "Email" IS DISTINCT FROM NULLIF(LOWER(TRIM("Email")), '')
                    OR "NormalizedEmail" IS DISTINCT FROM NULLIF(UPPER(TRIM("Email")), '')
                  );

                UPDATE "Users"
                SET "NormalizedEmail" = NULL
                WHERE "NormalizedEmail" IS NOT NULL AND TRIM("NormalizedEmail") = '';
                """);

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Professionals_LicenseNumber",
                table: "Professionals");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail",
                unique: true,
                filter: "\"NormalizedEmail\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_LicenseNumber",
                table: "Professionals",
                column: "LicenseNumber",
                unique: true,
                filter: "\"LicenseNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives",
                column: "DocumentNumber",
                unique: true,
                filter: "\"DocumentNumber\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Professionals_LicenseNumber",
                table: "Professionals");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_LicenseNumber",
                table: "Professionals",
                column: "LicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives",
                column: "DocumentNumber");
        }
    }
}
