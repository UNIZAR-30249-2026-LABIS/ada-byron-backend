using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReservaRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservas_espacios_EspacioCodigoEspacio",
                table: "reservas");

            migrationBuilder.DropIndex(
                name: "IX_reservas_EspacioCodigoEspacio",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "EspacioCodigoEspacio",
                table: "reservas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EspacioCodigoEspacio",
                table: "reservas",
                type: "character varying(20)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservas_EspacioCodigoEspacio",
                table: "reservas",
                column: "EspacioCodigoEspacio");

            migrationBuilder.AddForeignKey(
                name: "FK_reservas_espacios_EspacioCodigoEspacio",
                table: "reservas",
                column: "EspacioCodigoEspacio",
                principalTable: "espacios",
                principalColumn: "CodigoEspacio");
        }
    }
}
