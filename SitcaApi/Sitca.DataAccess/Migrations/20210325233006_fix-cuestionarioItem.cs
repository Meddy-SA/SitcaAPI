using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class fixcuestionarioItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuestionarioItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Texto = table.Column<string>(maxLength: 2500, nullable: true),
                    Nomenclatura = table.Column<string>(maxLength: 30, nullable: true),
                    ResultadoAuditor = table.Column<bool>(nullable: false),
                    Obligatorio = table.Column<bool>(nullable: false),
                    CuestionarioId = table.Column<int>(nullable: false),
                    PreguntaId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuestionarioItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuestionarioItem_Cuestionario_CuestionarioId",
                        column: x => x.CuestionarioId,
                        principalTable: "Cuestionario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuestionarioItem_Pregunta_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Pregunta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuestionarioItem_CuestionarioId",
                table: "CuestionarioItem",
                column: "CuestionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CuestionarioItem_PreguntaId",
                table: "CuestionarioItem",
                column: "PreguntaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuestionarioItem");
        }
    }
}
