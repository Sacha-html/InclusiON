using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonGuiSettings");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "PersonsWithDisability");

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSuperviseLogin",
                table: "ProfessionalPersons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "LearningStyle",
                table: "PersonsWithDisability",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AutonomyLevelId",
                table: "PersonsWithDisability",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoginMethodId",
                table: "PersonsWithDisability",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "PersonsWithDisability",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PictogramSequence",
                table: "PersonsWithDisability",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PinCodeHash",
                table: "PersonsWithDisability",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupervisorUserId",
                table: "PersonsWithDisability",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanSuperviseLogin",
                table: "PersonRepresentatives",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "ActivityResponses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ActivityAssignments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pendiente",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "AccessAudits",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "AutonomyLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RequiresSupervision = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutonomyLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_PersonsWithDisability_ForPersonId",
                        column: x => x.ForPersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invitations_Professionals_CreatedByProfessionalId",
                        column: x => x.CreatedByProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invitations_Users_UsedByUserId",
                        column: x => x.UsedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoginMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MinAutonomyLevel = table.Column<int>(type: "int", nullable: false),
                    RequiresEmail = table.Column<bool>(type: "bit", nullable: false),
                    RequiresPassword = table.Column<bool>(type: "bit", nullable: false),
                    RequiresPin = table.Column<bool>(type: "bit", nullable: false),
                    RequiresPictogram = table.Column<bool>(type: "bit", nullable: false),
                    RequiresSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrustedDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AuthorizedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrustedDevices_Users_AuthorizedByUserId",
                        column: x => x.AuthorizedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrustedDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AutonomyLevels",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Name", "RequiresSupervision" },
                values: new object[,]
                {
                    { 1, "Puede usar la aplicacion de forma independiente con login estandar", 1, true, "Alta", false },
                    { 2, "Requiere login simplificado (PIN o pictogramas) pero puede usar la app solo", 2, true, "Media", false },
                    { 3, "Requiere supervision y login asistido por familiar o profesional", 3, true, "Baja", true }
                });

            migrationBuilder.InsertData(
                table: "LoginMethods",
                columns: new[] { "Id", "Code", "Description", "DisplayOrder", "IsActive", "MinAutonomyLevel", "Name", "RequiresEmail", "RequiresPassword", "RequiresPictogram", "RequiresPin", "RequiresSupervisor" },
                values: new object[,]
                {
                    { 1, "STANDARD", "Login tradicional con email y contrasena", 1, true, 1, "Email y Contrasena", true, true, false, false, false },
                    { 2, "PIN", "Login con nombre de usuario y PIN de 4-6 digitos", 2, true, 2, "PIN Numerico", false, false, false, true, false },
                    { 3, "PICTOGRAM", "Login seleccionando una secuencia de imagenes", 3, true, 2, "Secuencia de Pictogramas", false, false, true, false, false },
                    { 4, "SUPERVISED", "Login requiere desbloqueo por familiar o profesional", 4, true, 3, "Supervisado", false, false, false, false, true },
                    { 5, "TRUSTED_DEVICE", "Login automatico en dispositivos previamente autorizados", 5, true, 3, "Dispositivo Confiable", false, false, false, false, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_AutonomyLevelId",
                table: "PersonsWithDisability",
                column: "AutonomyLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_LoginMethodId",
                table: "PersonsWithDisability",
                column: "LoginMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_SupervisorUserId",
                table: "PersonsWithDisability",
                column: "SupervisorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AutonomyLevels_Name",
                table: "AutonomyLevels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Code",
                table: "Invitations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_CreatedByProfessionalId",
                table: "Invitations",
                column: "CreatedByProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Email",
                table: "Invitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ForPersonId",
                table: "Invitations",
                column: "ForPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_UsedByUserId",
                table: "Invitations",
                column: "UsedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginMethods_Code",
                table: "LoginMethods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrustedDevices_AuthorizedByUserId",
                table: "TrustedDevices",
                column: "AuthorizedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedDevices_DeviceId",
                table: "TrustedDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedDevices_UserId_DeviceId",
                table: "TrustedDevices",
                columns: new[] { "UserId", "DeviceId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonsWithDisability_AutonomyLevels_AutonomyLevelId",
                table: "PersonsWithDisability",
                column: "AutonomyLevelId",
                principalTable: "AutonomyLevels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonsWithDisability_LoginMethods_LoginMethodId",
                table: "PersonsWithDisability",
                column: "LoginMethodId",
                principalTable: "LoginMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonsWithDisability_Users_SupervisorUserId",
                table: "PersonsWithDisability",
                column: "SupervisorUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonsWithDisability_AutonomyLevels_AutonomyLevelId",
                table: "PersonsWithDisability");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonsWithDisability_LoginMethods_LoginMethodId",
                table: "PersonsWithDisability");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonsWithDisability_Users_SupervisorUserId",
                table: "PersonsWithDisability");

            migrationBuilder.DropTable(
                name: "AutonomyLevels");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "LoginMethods");

            migrationBuilder.DropTable(
                name: "TrustedDevices");

            migrationBuilder.DropIndex(
                name: "IX_PersonsWithDisability_AutonomyLevelId",
                table: "PersonsWithDisability");

            migrationBuilder.DropIndex(
                name: "IX_PersonsWithDisability_LoginMethodId",
                table: "PersonsWithDisability");

            migrationBuilder.DropIndex(
                name: "IX_PersonsWithDisability_SupervisorUserId",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanSuperviseLogin",
                table: "ProfessionalPersons");

            migrationBuilder.DropColumn(
                name: "AutonomyLevelId",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "LoginMethodId",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "PictogramSequence",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "PinCodeHash",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "SupervisorUserId",
                table: "PersonsWithDisability");

            migrationBuilder.DropColumn(
                name: "CanSuperviseLogin",
                table: "PersonRepresentatives");

            migrationBuilder.AlterColumn<int>(
                name: "LearningStyle",
                table: "PersonsWithDisability",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "PersonsWithDisability",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Result",
                table: "ActivityResponses",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ActivityAssignments",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pendiente");

            migrationBuilder.AlterColumn<int>(
                name: "ActionType",
                table: "AccessAudits",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateTable(
                name: "PersonGuiSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AutoAdvance = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AutoAdvanceDelay = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    ColorTheme = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DarkMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EasyReadingMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FontSize = table.Column<int>(type: "int", nullable: false, defaultValue: 16),
                    HighContrastMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LargeButtons = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReducedMotion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ShowImages = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowPictograms = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowVideos = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SimplifiedNavigation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SoundEffectsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TextToSpeechEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TextToSpeechSpeed = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    TouchTargetSize = table.Column<int>(type: "int", nullable: false, defaultValue: 44),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Volume = table.Column<int>(type: "int", nullable: false, defaultValue: 80)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonGuiSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonGuiSettings_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonGuiSettings_PersonId",
                table: "PersonGuiSettings",
                column: "PersonId",
                unique: true);
        }
    }
}
