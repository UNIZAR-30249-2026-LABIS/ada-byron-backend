using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEdificioConfigPBI6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EdificioConfigId",
                table: "espacios",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "edificio_config",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PorcentajeOcupacion = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edificio_config", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "edificio_config",
                columns: new[] { "Id", "PorcentajeOcupacion" },
                values: new object[] { "AdaByron", 100.0 });

            migrationBuilder.CreateIndex(
                name: "IX_espacios_EdificioConfigId",
                table: "espacios",
                column: "EdificioConfigId");

            migrationBuilder.AddForeignKey(
                name: "FK_espacios_edificio_config_EdificioConfigId",
                table: "espacios",
                column: "EdificioConfigId",
                principalTable: "edificio_config",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_espacios_edificio_config_EdificioConfigId",
                table: "espacios");

            migrationBuilder.DropTable(
                name: "edificio_config");

            migrationBuilder.DropIndex(
                name: "IX_espacios_EdificioConfigId",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "EdificioConfigId",
                table: "espacios");
        }
    }
}
