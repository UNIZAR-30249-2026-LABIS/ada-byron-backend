using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlantaToAltura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "planta",
                table: "espacios",
                newName: "altura");

            migrationBuilder.RenameIndex(
                name: "IX_espacios_planta",
                table: "espacios",
                newName: "IX_espacios_altura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "altura",
                table: "espacios",
                newName: "planta");

            migrationBuilder.RenameIndex(
                name: "IX_espacios_altura",
                table: "espacios",
                newName: "IX_espacios_planta");
        }
    }
}
