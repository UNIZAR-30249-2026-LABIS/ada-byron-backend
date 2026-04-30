using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Persistence.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(AdaByron.Infrastructure.Persistence.DbContext.AplicacionDbContext))]
    [Migration("20260419194000_AddSpaceReservationSettings")]
    public partial class AddSpaceReservationSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "es_reservable",
                table: "espacios",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "horario_reserva",
                table: "espacios",
                type: "text",
                nullable: false,
                defaultValue: "[{\"diaSemana\":0,\"activo\":true,\"horaInicio\":\"00:00\",\"horaFin\":\"23:59\"},{\"diaSemana\":1,\"activo\":true,\"horaInicio\":\"00:00\",\"horaFin\":\"23:59\"},{\"diaSemana\":2,\"activo\":true,\"horaInicio\":\"00:00\",\"horaFin\":\"23:59\"},{\"diaSemana\":3,\"activo\":true,\"horaInicio\":\"00:00\",\"horaFin\":\"23:59\"},{\"diaSemana\":4,\"activo\":true,\"horaInicio\":\"00:00\",\"horaFin\":\"23:59\"},{\"diaSemana\":5,\"activo\":true,\"horaInicio\":\"00:00\",\"horaFin\":\"23:59\"},{\"diaSemana\":6,\"activo\":true,\"horaInicio\":\"00:00\",\"horaFin\":\"23:59\"}]");

            migrationBuilder.Sql("""
                UPDATE espacios
                SET es_reservable = CASE
                    WHEN "TipoFisico" = 'Despacho' THEN FALSE
                    ELSE TRUE
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "es_reservable",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "horario_reserva",
                table: "espacios");
        }
    }
}
