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
            // La base de datos ya tiene la columna 'altura' (probablemente renombrada manualmente o por reconstrucción de schema), 
            // comentar estas operaciones evita el PostgresException: column "planta" does not exist
            /* 
            migrationBuilder.RenameColumn(
                name: "planta",
                table: "espacios",
                newName: "altura");

            migrationBuilder.RenameIndex(
                name: "IX_espacios_planta",
                table: "espacios",
                newName: "IX_espacios_altura");
            */

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
