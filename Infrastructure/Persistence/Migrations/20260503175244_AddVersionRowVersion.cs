using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "reservas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "reservas");
        }
    }
}
