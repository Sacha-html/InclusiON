using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class CatalogActivityAssignmentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear tabla de catálogo
            migrationBuilder.CreateTable(
                name: "ActivityAssignmentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityAssignmentStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAssignmentStatuses_Name",
                table: "ActivityAssignmentStatuses",
                column: "Name",
                unique: true);

            // 2. Insertar seed data
            migrationBuilder.InsertData(
                table: "ActivityAssignmentStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pendiente" },
                    { 2, "EnProgreso" },
                    { 3, "Completada" },
                    { 4, "Cancelada" }
                });

            // 3. Agregar StatusId nullable para poder migrar datos primero
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "ActivityAssignments",
                type: "integer",
                nullable: true);

            // 4. Migrar datos: Status varchar → StatusId int
            migrationBuilder.Sql(@"
                UPDATE ""ActivityAssignments"" SET ""StatusId"" =
                    CASE ""Status""
                        WHEN 'Pendiente'  THEN 1
                        WHEN 'EnProgreso' THEN 2
                        WHEN 'Completada' THEN 3
                        WHEN 'Cancelada'  THEN 4
                        ELSE 1
                    END;
            ");

            // 5. Hacer StatusId NOT NULL con default 1
            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "ActivityAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // 6. Agregar FK e índice
            migrationBuilder.CreateIndex(
                name: "IX_ActivityAssignments_StatusId",
                table: "ActivityAssignments",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityAssignments_ActivityAssignmentStatuses_StatusId",
                table: "ActivityAssignments",
                column: "StatusId",
                principalTable: "ActivityAssignmentStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 7. Eliminar columna y índice viejos
            migrationBuilder.DropIndex(
                name: "IX_ActivityAssignments_Status",
                table: "ActivityAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ActivityAssignments");

            // 8. Ajuste de columna Result en ActivityResponses
            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "ActivityResponses",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityAssignments_ActivityAssignmentStatuses_StatusId",
                table: "ActivityAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ActivityAssignments_StatusId",
                table: "ActivityAssignments");

            // Restaurar columna Status como nullable primero
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ActivityAssignments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Migrar datos de vuelta: StatusId → Status varchar
            migrationBuilder.Sql(@"
                UPDATE ""ActivityAssignments"" SET ""Status"" =
                    CASE ""StatusId""
                        WHEN 1 THEN 'Pendiente'
                        WHEN 2 THEN 'EnProgreso'
                        WHEN 3 THEN 'Completada'
                        WHEN 4 THEN 'Cancelada'
                        ELSE 'Pendiente'
                    END;
            ");

            // Hacer Status NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ActivityAssignments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pendiente",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityAssignments_Status",
                table: "ActivityAssignments",
                column: "Status");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "ActivityAssignments");

            migrationBuilder.DropTable(
                name: "ActivityAssignmentStatuses");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "ActivityResponses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
