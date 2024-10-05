using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class seccionmodulo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeccionModuloId",
                table: "Pregunta",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SeccionModulo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    Orden = table.Column<string>(maxLength: 5, nullable: true),
                    ModuloId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeccionModulo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeccionModulo_Modulo_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pregunta_SeccionModuloId",
                table: "Pregunta",
                column: "SeccionModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_SeccionModulo_ModuloId",
                table: "SeccionModulo",
                column: "ModuloId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pregunta_SeccionModulo_SeccionModuloId",
                table: "Pregunta",
                column: "SeccionModuloId",
                principalTable: "SeccionModulo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pregunta_SeccionModulo_SeccionModuloId",
                table: "Pregunta");

            migrationBuilder.DropTable(
                name: "SeccionModulo");

            migrationBuilder.DropIndex(
                name: "IX_Pregunta_SeccionModuloId",
                table: "Pregunta");

            migrationBuilder.DropColumn(
                name: "SeccionModuloId",
                table: "Pregunta");
        }
    }
}
