using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImportEdificioConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_espacios_edificio_config_EdificioConfigId",
                table: "espacios");

            migrationBuilder.DropIndex(
                name: "IX_espacios_EdificioConfigId",
                table: "espacios");

            migrationBuilder.DeleteData(
                table: "edificio_config",
                keyColumn: "Id",
                keyValue: "AdaByron");

            migrationBuilder.DropColumn(
                name: "EdificioConfigId",
                table: "espacios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EdificioConfigId",
                table: "espacios",
                type: "text",
                nullable: true);

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
    }
}
