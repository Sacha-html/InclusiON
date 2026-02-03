using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_FirstName",
                table: "Professionals",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_IsActive_FirstName",
                table: "Professionals",
                columns: new[] { "IsActive", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_LastName",
                table: "Professionals",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_LicenseNumber",
                table: "Professionals",
                column: "LicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_FirstName",
                table: "PersonsWithDisability",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_IsActive_FirstName",
                table: "PersonsWithDisability",
                columns: new[] { "IsActive", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_IsActive_LastName",
                table: "PersonsWithDisability",
                columns: new[] { "IsActive", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_LastName",
                table: "PersonsWithDisability",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_FirstName",
                table: "FamilyRepresentatives",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_IsActive_FirstName",
                table: "FamilyRepresentatives",
                columns: new[] { "IsActive", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_LastName",
                table: "FamilyRepresentatives",
                column: "LastName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Professionals_FirstName",
                table: "Professionals");

            migrationBuilder.DropIndex(
                name: "IX_Professionals_IsActive_FirstName",
                table: "Professionals");

            migrationBuilder.DropIndex(
                name: "IX_Professionals_LastName",
                table: "Professionals");

            migrationBuilder.DropIndex(
                name: "IX_Professionals_LicenseNumber",
                table: "Professionals");

            migrationBuilder.DropIndex(
                name: "IX_PersonsWithDisability_FirstName",
                table: "PersonsWithDisability");

            migrationBuilder.DropIndex(
                name: "IX_PersonsWithDisability_IsActive_FirstName",
                table: "PersonsWithDisability");

            migrationBuilder.DropIndex(
                name: "IX_PersonsWithDisability_IsActive_LastName",
                table: "PersonsWithDisability");

            migrationBuilder.DropIndex(
                name: "IX_PersonsWithDisability_LastName",
                table: "PersonsWithDisability");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRepresentatives_FirstName",
                table: "FamilyRepresentatives");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRepresentatives_IsActive_FirstName",
                table: "FamilyRepresentatives");

            migrationBuilder.DropIndex(
                name: "IX_FamilyRepresentatives_LastName",
                table: "FamilyRepresentatives");
        }
    }
}
