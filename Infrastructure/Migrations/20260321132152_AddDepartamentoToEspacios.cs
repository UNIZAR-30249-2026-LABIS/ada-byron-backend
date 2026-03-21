using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartamentoToEspacios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroAsistentes",
                table: "reservas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "reservas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "espacios",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroAsistentes",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "espacios");
        }
    }
}
