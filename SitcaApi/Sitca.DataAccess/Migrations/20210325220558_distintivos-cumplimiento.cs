using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class distintivoscumplimiento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Distintivo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 40, nullable: true),
                    File = table.Column<string>(maxLength: 60, nullable: true),
                    Activo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distintivo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cumplimiento",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PorcentajeMinimo = table.Column<int>(nullable: false),
                    PorcentajeMaximo = table.Column<int>(nullable: false),
                    ModuloId = table.Column<int>(nullable: false),
                    DistintivoId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cumplimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cumplimiento_Distintivo_DistintivoId",
                        column: x => x.DistintivoId,
                        principalTable: "Distintivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cumplimiento_Modulo_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cumplimiento_DistintivoId",
                table: "Cumplimiento",
                column: "DistintivoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cumplimiento_ModuloId",
                table: "Cumplimiento",
                column: "ModuloId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cumplimiento");

            migrationBuilder.DropTable(
                name: "Distintivo");
        }
    }
}
