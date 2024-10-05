using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class fixcuestionario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AsesorId",
                table: "Cuestionario",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditorId",
                table: "Cuestionario",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsesorId",
                table: "Cuestionario");

            migrationBuilder.DropColumn(
                name: "AuditorId",
                table: "Cuestionario");
        }
    }
}
