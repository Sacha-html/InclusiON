using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DisabilityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisabilityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EducationalInstitutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalInstitutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Surname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    LastLoginUserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyRepresentatives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyRepresentatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyRepresentatives_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonsWithDisability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisabilityTypeId = table.Column<int>(type: "int", nullable: true),
                    AttentionLevel = table.Column<int>(type: "int", nullable: true),
                    CommunicationLevel = table.Column<int>(type: "int", nullable: true),
                    UsesAAC = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsesSignLanguage = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MotorSkillLevel = table.Column<int>(type: "int", nullable: true),
                    InterestsAndMotivators = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LearningStyle = table.Column<int>(type: "int", nullable: true),
                    AvailableResources = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AdditionalTherapies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresLargeFont = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RequiresHighContrast = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VisualNoiseSensitivity = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SoundSensitivity = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonsWithDisability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonsWithDisability_DisabilityTypes_DisabilityTypeId",
                        column: x => x.DisabilityTypeId,
                        principalTable: "DisabilityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonsWithDisability_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Professionals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Specialty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Professionals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RevokedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessedPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    AffectedTable = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AffectedRecordId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessAudits_PersonsWithDisability_AccessedPersonId",
                        column: x => x.AccessedPersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessAudits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelatedPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ParentMessageId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Messages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_PersonsWithDisability_RelatedPersonId",
                        column: x => x.RelatedPersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonGuiSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FontSize = table.Column<int>(type: "int", nullable: false, defaultValue: 16),
                    HighContrastMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ColorTheme = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DarkMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ReducedMotion = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TextToSpeechEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TextToSpeechSpeed = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    SoundEffectsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Volume = table.Column<int>(type: "int", nullable: false, defaultValue: 80),
                    SimplifiedNavigation = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LargeButtons = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TouchTargetSize = table.Column<int>(type: "int", nullable: false, defaultValue: 44),
                    AutoAdvance = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AutoAdvanceDelay = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    ShowPictograms = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowImages = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ShowVideos = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EasyReadingMode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
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

            migrationBuilder.CreateTable(
                name: "PersonRepresentatives",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RepresentativeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasInformedConsent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConsentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRepresentatives", x => new { x.PersonId, x.RepresentativeId });
                    table.ForeignKey(
                        name: "FK_PersonRepresentatives_FamilyRepresentatives_RepresentativeId",
                        column: x => x.RepresentativeId,
                        principalTable: "FamilyRepresentatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonRepresentatives_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    Instructions = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    HasVisualSupport = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    HasAudioSupport = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsesEasyReading = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsesPictograms = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResourcesUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    ComplexityLevel = table.Column<int>(type: "int", nullable: true),
                    RequiresSupervision = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsStandardActivity = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_ActivityCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ActivityCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Activities_Professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Diagnoses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiagnosisDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrimaryDiagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InitialObservations = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    IdentifiedCapabilities = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    IdentifiedChallenges = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    RequiredSupports = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    PedagogicalObjectives = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    RecommendedStrategies = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diagnoses_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Diagnoses_Professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalInstitutions",
                columns: table => new
                {
                    ProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstitutionId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalInstitutions", x => new { x.ProfessionalId, x.InstitutionId });
                    table.ForeignKey(
                        name: "FK_ProfessionalInstitutions_EducationalInstitutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "EducationalInstitutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfessionalInstitutions_Professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalPersons",
                columns: table => new
                {
                    ProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPrimaryProfessional = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalPersons", x => new { x.ProfessionalId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_ProfessionalPersons_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfessionalPersons_Professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportTypeId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    ReportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AchievedGoals = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    AreasToReinforce = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    FutureRecommendations = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    NextObjectives = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_Professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_ReportTypes_ReportTypeId",
                        column: x => x.ReportTypeId,
                        principalTable: "ReportTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedByProfessionalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    SequenceOrder = table.Column<int>(type: "int", nullable: true),
                    IsEvaluationActivity = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityAssignments_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityAssignments_PersonsWithDisability_PersonId",
                        column: x => x.PersonId,
                        principalTable: "PersonsWithDisability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityAssignments_Professionals_AssignedByProfessionalId",
                        column: x => x.AssignedByProfessionalId,
                        principalTable: "Professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimeSpentSeconds = table.Column<int>(type: "int", nullable: true),
                    Result = table.Column<int>(type: "int", nullable: true),
                    SuccessPercentage = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ResponsePattern = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    RequiredSupport = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FrustrationLevel = table.Column<int>(type: "int", nullable: true),
                    Observations = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityResponses_ActivityAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ActivityAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ActivityCategories",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Actividades de lectura, escritura, conciencia fonológica y comprensión lectora", true, "Lectoescritura" },
                    { 2, "Actividades prenuméricas, numeración, secuencias y operaciones básicas", true, "Numeración y Matemática" },
                    { 3, "Modificación de conducta, hábitos, rutinas, normas de convivencia e historias sociales", true, "Habilidades Socioemocionales" },
                    { 4, "Lengua de señas, sistemas aumentativos y alternativos de comunicación (SAAC)", true, "Comunicación y Lenguaje" },
                    { 5, "Motricidad fina, motricidad gruesa, coordinación óculo-manual y orientación espacial", true, "Motricidad y Coordinación" },
                    { 6, "Música, plástica y dramatización", true, "Creatividad y Expresión Artística" },
                    { 7, "Vestimenta, higiene, manejo del dinero, noción del tiempo y situaciones cotidianas", true, "Autonomía y Vida Diaria" },
                    { 8, "Memoria, atención, percepción y resolución de problemas", true, "Estimulación Cognitiva" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), null, "Admin", "ADMIN" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, "Professional", "PROFESSIONAL" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, "FamilyRepresentative", "FAMILYREPRESENTATIVE" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, "PersonWithDisability", "PERSONWITHDISABILITY" }
                });

            migrationBuilder.InsertData(
                table: "DisabilityTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Incluye discapacidad auditiva y visual (sordera, ceguera)", true, "Sensorial" },
                    { 2, "Relacionada con el aprendizaje y habilidades adaptativas de la vida diaria", true, "Cognitiva/Intelectual" },
                    { 3, "Alteraciones en el funcionamiento motor", true, "Motriz" },
                    { 4, "Derivada de diagnósticos vinculados a la salud mental", true, "Mental/Psicosocial" },
                    { 5, "Combinación de dos o más tipos de discapacidad", true, "Múltiple" }
                });

            migrationBuilder.InsertData(
                table: "ReportTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Informe de estandarización y diagnóstico inicial del estudiante", true, "Evaluación Inicial" },
                    { 2, "Informe de progreso mensual", true, "Seguimiento Mensual" },
                    { 3, "Evaluación de progreso trimestral", true, "Informe Trimestral" },
                    { 4, "Proyecto Pedagógico Individual para la inclusión", true, "PPI" },
                    { 5, "Resumen anual de logros alcanzados y áreas a reforzar", true, "Informe Anual" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessAudits_AccessedPersonId",
                table: "AccessAudits",
                column: "AccessedPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessAudits_ActionType",
                table: "AccessAudits",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AccessAudits_Timestamp",
                table: "AccessAudits",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AccessAudits_UserId",
                table: "AccessAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_CategoryId",
                table: "Activities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ProfessionalId",
                table: "Activities",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAssignments_ActivityId",
                table: "ActivityAssignments",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAssignments_AssignedByProfessionalId",
                table: "ActivityAssignments",
                column: "AssignedByProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAssignments_PersonId",
                table: "ActivityAssignments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAssignments_Status",
                table: "ActivityAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCategories_Name",
                table: "ActivityCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityResponses_AssignmentId",
                table: "ActivityResponses",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_PersonId",
                table: "Diagnoses",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnoses_ProfessionalId",
                table: "Diagnoses",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_DisabilityTypes_Name",
                table: "DisabilityTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_DocumentNumber",
                table: "FamilyRepresentatives",
                column: "DocumentNumber",
                unique: true,
                filter: "[DocumentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyRepresentatives_UserId",
                table: "FamilyRepresentatives",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ParentMessageId",
                table: "Messages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RelatedPersonId",
                table: "Messages",
                column: "RelatedPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentAt",
                table: "Messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonGuiSettings_PersonId",
                table: "PersonGuiSettings",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonRepresentatives_RepresentativeId",
                table: "PersonRepresentatives",
                column: "RepresentativeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_DisabilityTypeId",
                table: "PersonsWithDisability",
                column: "DisabilityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_DocumentNumber",
                table: "PersonsWithDisability",
                column: "DocumentNumber",
                unique: true,
                filter: "[DocumentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonsWithDisability_UserId",
                table: "PersonsWithDisability",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalInstitutions_InstitutionId",
                table: "ProfessionalInstitutions",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalPersons_PersonId",
                table: "ProfessionalPersons",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_DocumentNumber",
                table: "Professionals",
                column: "DocumentNumber",
                unique: true,
                filter: "[DocumentNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Professionals_UserId",
                table: "Professionals",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_IsActive",
                table: "RefreshTokens",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_PersonId",
                table: "Reports",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ProfessionalId",
                table: "Reports",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportTypeId",
                table: "Reports",
                column: "ReportTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTypes_Name",
                table: "ReportTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessAudits");

            migrationBuilder.DropTable(
                name: "ActivityResponses");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Diagnoses");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "PersonGuiSettings");

            migrationBuilder.DropTable(
                name: "PersonRepresentatives");

            migrationBuilder.DropTable(
                name: "ProfessionalInstitutions");

            migrationBuilder.DropTable(
                name: "ProfessionalPersons");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ActivityAssignments");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "FamilyRepresentatives");

            migrationBuilder.DropTable(
                name: "EducationalInstitutions");

            migrationBuilder.DropTable(
                name: "ReportTypes");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "PersonsWithDisability");

            migrationBuilder.DropTable(
                name: "ActivityCategories");

            migrationBuilder.DropTable(
                name: "Professionals");

            migrationBuilder.DropTable(
                name: "DisabilityTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
