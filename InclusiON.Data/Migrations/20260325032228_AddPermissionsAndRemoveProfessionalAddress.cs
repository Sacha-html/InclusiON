using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionsAndRemoveProfessionalAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Professionals");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21,
                column: "ClaimValue",
                value: "diagnoses:read");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22,
                column: "ClaimValue",
                value: "reports:read");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23,
                column: "ClaimValue",
                value: "reports:create");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24,
                column: "ClaimValue",
                value: "reports:export");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25,
                column: "ClaimValue",
                value: "messages:read");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26,
                column: "ClaimValue",
                value: "messages:create");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "invitations:read", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "invitations:create", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "institutions:read", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "institutions:create", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "institutions:update", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "settings:read", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "settings:update", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "audit:read", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35,
                column: "ClaimValue",
                value: "persons:read");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "persons:update", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37,
                column: "RoleId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "activities:create", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "activities:update", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "diagnoses:read", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "diagnoses:create", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "diagnoses:update", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "reports:read", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 44, "permission", "reports:create", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 45, "permission", "messages:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 46, "permission", "messages:create", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 47, "permission", "invitations:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 48, "permission", "invitations:create", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 49, "permission", "persons:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 50, "permission", "activities:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 51, "permission", "diagnoses:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 52, "permission", "reports:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 53, "permission", "messages:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 54, "permission", "messages:create", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 55, "permission", "activities:read", new Guid("44444444-4444-4444-4444-444444444444") },
                    { 56, "permission", "activities:respond", new Guid("44444444-4444-4444-4444-444444444444") },
                    { 57, "permission", "messages:read", new Guid("44444444-4444-4444-4444-444444444444") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Professionals",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21,
                column: "ClaimValue",
                value: "reports:read");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22,
                column: "ClaimValue",
                value: "reports:create");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23,
                column: "ClaimValue",
                value: "reports:export");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24,
                column: "ClaimValue",
                value: "settings:read");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25,
                column: "ClaimValue",
                value: "settings:update");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26,
                column: "ClaimValue",
                value: "audit:read");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "persons:read", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "persons:update", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "activities:read", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "activities:create", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "activities:update", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "reports:read", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "reports:create", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "messages:read", new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35,
                column: "ClaimValue",
                value: "messages:create");

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "persons:read", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37,
                column: "RoleId",
                value: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "reports:read", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "messages:read", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "messages:create", new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "activities:read", new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "activities:respond", new Guid("44444444-4444-4444-4444-444444444444") });

            migrationBuilder.UpdateData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ClaimValue", "RoleId" },
                values: new object[] { "messages:read", new Guid("44444444-4444-4444-4444-444444444444") });
        }
    }
}
