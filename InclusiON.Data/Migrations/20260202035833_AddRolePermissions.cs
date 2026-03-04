using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "permission", "users:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 2, "permission", "users:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 3, "permission", "users:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 4, "permission", "users:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 5, "permission", "persons:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 6, "permission", "persons:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 7, "permission", "persons:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 8, "permission", "persons:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 9, "permission", "professionals:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 10, "permission", "professionals:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 11, "permission", "professionals:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 12, "permission", "professionals:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 13, "permission", "family:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 14, "permission", "family:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 15, "permission", "family:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 16, "permission", "family:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 17, "permission", "activities:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 18, "permission", "activities:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 19, "permission", "activities:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 20, "permission", "activities:delete", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 21, "permission", "reports:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 22, "permission", "reports:create", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 23, "permission", "reports:export", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 24, "permission", "settings:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 25, "permission", "settings:update", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 26, "permission", "audit:read", new Guid("11111111-1111-1111-1111-111111111111") },
                    { 27, "permission", "persons:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 28, "permission", "persons:update", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 29, "permission", "activities:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 30, "permission", "activities:create", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 31, "permission", "activities:update", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 32, "permission", "reports:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 33, "permission", "reports:create", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 34, "permission", "messages:read", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 35, "permission", "messages:create", new Guid("22222222-2222-2222-2222-222222222222") },
                    { 36, "permission", "persons:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 37, "permission", "activities:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 38, "permission", "reports:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 39, "permission", "messages:read", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 40, "permission", "messages:create", new Guid("33333333-3333-3333-3333-333333333333") },
                    { 41, "permission", "activities:read", new Guid("44444444-4444-4444-4444-444444444444") },
                    { 42, "permission", "activities:respond", new Guid("44444444-4444-4444-4444-444444444444") },
                    { 43, "permission", "messages:read", new Guid("44444444-4444-4444-4444-444444444444") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 43);
        }
    }
}
