using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class empresacamposnuevos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdNacional",
                table: "Empresa",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreRepresentante",
                table: "Empresa",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Empresa",
                maxLength: 15,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdNacional",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "NombreRepresentante",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Empresa");
        }
    }
}
