using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class modulopreguntas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Modulo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<int>(maxLength: 50, nullable: false),
                    Transversal = table.Column<bool>(nullable: false),
                    IdTipologia = table.Column<int>(nullable: false),
                    Orden = table.Column<int>(nullable: false),
                    Nomenclatura = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pregunta",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texto = table.Column<string>(maxLength: 2500, nullable: true),
                    NoAplica = table.Column<bool>(nullable: false),
                    Obligatoria = table.Column<bool>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    Nomenclatura = table.Column<string>(maxLength: 20, nullable: true),
                    Orden = table.Column<string>(maxLength: 5, nullable: true),
                    IdModulo = table.Column<int>(nullable: false),
                    ModuloId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pregunta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pregunta_Modulo_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pregunta_ModuloId",
                table: "Pregunta",
                column: "ModuloId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pregunta");

            migrationBuilder.DropTable(
                name: "Modulo");
        }
    }
}
