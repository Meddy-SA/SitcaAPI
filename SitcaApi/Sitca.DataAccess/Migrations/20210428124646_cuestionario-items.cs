using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class cuestionarioitems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdAsesor",
                table: "Cuestionario");

            migrationBuilder.DropColumn(
                name: "IdAuditor",
                table: "Cuestionario");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAsesor",
                table: "Cuestionario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdAuditor",
                table: "Cuestionario",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
