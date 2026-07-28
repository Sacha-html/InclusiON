using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InclusiON.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPgvectorExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");

            migrationBuilder.DropColumn(
                name: "EmbeddingJson",
                table: "ActivityEmbeddings");

            migrationBuilder.Sql(
                "ALTER TABLE \"ActivityEmbeddings\" ADD COLUMN \"Embedding\" vector(384);");

            migrationBuilder.Sql(
                "CREATE INDEX ON \"ActivityEmbeddings\" USING hnsw (\"Embedding\" vector_cosine_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"ActivityEmbeddings_Embedding_idx\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"ActivityEmbeddings\" DROP COLUMN IF EXISTS \"Embedding\";");

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingJson",
                table: "ActivityEmbeddings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
