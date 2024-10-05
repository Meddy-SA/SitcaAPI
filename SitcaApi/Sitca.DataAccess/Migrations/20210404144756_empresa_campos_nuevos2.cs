using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class empresa_campos_nuevos2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CargoRepresentante",
                table: "Empresa",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "Empresa",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoRepresentante",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Empresa");
        }
    }
}
