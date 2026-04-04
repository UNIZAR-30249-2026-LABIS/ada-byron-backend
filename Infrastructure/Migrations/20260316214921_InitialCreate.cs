using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AdaByron.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "espacios",
                columns: table => new
                {
                    CodigoEspacio = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    planta = table.Column<int>(type: "integer", nullable: false),
                    aforo = table.Column<int>(type: "integer", nullable: false),
                    TipoFisico = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CategoriaReserva = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ubicacion = table.Column<Point>(type: "geometry(Point,4326)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_espacios", x => x.CodigoEspacio);
                });

            migrationBuilder.CreateTable(
                name: "personas",
                columns: table => new
                {
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Rol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Departamento = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonaId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EspacioId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reservas_espacios_EspacioId",
                        column: x => x.EspacioId,
                        principalTable: "espacios",
                        principalColumn: "CodigoEspacio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reservas_personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "personas",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reservas_EspacioId",
                table: "reservas",
                column: "EspacioId");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_PersonaId",
                table: "reservas",
                column: "PersonaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservas");

            migrationBuilder.DropTable(
                name: "espacios");

            migrationBuilder.DropTable(
                name: "personas");
        }
    }
}
