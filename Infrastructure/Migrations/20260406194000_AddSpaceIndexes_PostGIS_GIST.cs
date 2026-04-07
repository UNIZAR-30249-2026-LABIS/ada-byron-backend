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
            migrationBuilder.CreateIndex(
                name: "IX_espacios_planta",
                table: "espacios",
                column: "planta");

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
                name: "IX_espacios_planta",
                table: "espacios");

            migrationBuilder.DropIndex(
                name: "IX_espacios_ubicacion",
                table: "espacios");
        }
    }
}
