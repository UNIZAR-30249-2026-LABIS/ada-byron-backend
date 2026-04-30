using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdaByron.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEdificioConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_espacios_altura",
                table: "espacios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_espacios_altura",
                table: "espacios",
                column: "altura");
        }
    }
}
