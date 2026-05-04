using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRolesAndSpaceAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "personas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "TipoAsignacion",
                table: "espacios",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Eina");

            migrationBuilder.AddColumn<string>(
                name: "personas_asignadas",
                table: "espacios",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql("""
                UPDATE personas
                SET "Rol" = CASE "Rol"
                    WHEN 'Estudiante' THEN '["Estudiante"]'
                    WHEN 'TecnicoLab' THEN '["TecnicoLaboratorio"]'
                    WHEN 'Docente' THEN '["DocenteInvestigador"]'
                    WHEN 'Conserje' THEN '["Conserje"]'
                    WHEN 'Gerente' THEN '["Gerente"]'
                    ELSE '["Estudiante"]'
                END;
                """);

            migrationBuilder.Sql("""
                UPDATE espacios
                SET "TipoAsignacion" = CASE
                    WHEN "TipoFisico" IN ('Aula', 'SalaComun') THEN 'Eina'
                    WHEN COALESCE("Departamento", '') <> '' THEN 'Departamento'
                    WHEN "TipoFisico" = 'Despacho' THEN 'Personas'
                    ELSE 'Eina'
                END,
                personas_asignadas = '[]';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoAsignacion",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "personas_asignadas",
                table: "espacios");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "personas",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
