using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
  public partial class EmpresaUpdate : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "Empresa",
          columns: table => new
          {
            Id = table.Column<int>(nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Nombre = table.Column<string>(maxLength: 60, nullable: true),
            Calle = table.Column<string>(maxLength: 60, nullable: true),
            Numero = table.Column<string>(maxLength: 60, nullable: true),
            Direccion = table.Column<string>(maxLength: 60, nullable: true),
            Longitud = table.Column<string>(maxLength: 20, nullable: true),
            Latitud = table.Column<string>(maxLength: 20, nullable: true),
            IdPais = table.Column<int>(nullable: false),
            Active = table.Column<bool>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Empresa", x => x.Id);
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Empresa");
    }
  }
}
