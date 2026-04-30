using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpaceIndexes_PostGIS_GIST : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE espacios RENAME COLUMN planta TO altura;");

            migrationBuilder.CreateIndex(
                name: "IX_espacios_altura",
                table: "espacios",
                column: "altura");

            migrationBuilder.CreateIndex(
                name: "IX_espacios_ubicacion",
                table: "espacios",
                column: "ubicacion")
                .Annotation("Npgsql:IndexMethod", "GIST");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_espacios_altura",
                table: "espacios");

            migrationBuilder.DropIndex(
                name: "IX_espacios_ubicacion",
                table: "espacios");
        }
    }
}
