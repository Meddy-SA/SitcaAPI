using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class subtituloscert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubtituloSeccionId",
                table: "Pregunta",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Prueba",
                table: "Cuestionario",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SubtituloSeccion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Orden = table.Column<string>(maxLength: 5, nullable: true),
                    Nomenclatura = table.Column<string>(maxLength: 10, nullable: true),
                    SeccionModuloId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubtituloSeccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubtituloSeccion_SeccionModulo_SeccionModuloId",
                        column: x => x.SeccionModuloId,
                        principalTable: "SeccionModulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pregunta_SubtituloSeccionId",
                table: "Pregunta",
                column: "SubtituloSeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubtituloSeccion_SeccionModuloId",
                table: "SubtituloSeccion",
                column: "SeccionModuloId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pregunta_SubtituloSeccion_SubtituloSeccionId",
                table: "Pregunta",
                column: "SubtituloSeccionId",
                principalTable: "SubtituloSeccion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pregunta_SubtituloSeccion_SubtituloSeccionId",
                table: "Pregunta");

            migrationBuilder.DropTable(
                name: "SubtituloSeccion");

            migrationBuilder.DropIndex(
                name: "IX_Pregunta_SubtituloSeccionId",
                table: "Pregunta");

            migrationBuilder.DropColumn(
                name: "SubtituloSeccionId",
                table: "Pregunta");

            migrationBuilder.DropColumn(
                name: "Prueba",
                table: "Cuestionario");
        }
    }
}
