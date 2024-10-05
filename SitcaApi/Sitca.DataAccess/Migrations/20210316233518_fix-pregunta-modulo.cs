using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class fixpreguntamodulo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pregunta_Modulo_ModuloId",
                table: "Pregunta");

            migrationBuilder.DropColumn(
                name: "IdModulo",
                table: "Pregunta");

            migrationBuilder.AlterColumn<int>(
                name: "ModuloId",
                table: "Pregunta",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pregunta_Modulo_ModuloId",
                table: "Pregunta",
                column: "ModuloId",
                principalTable: "Modulo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pregunta_Modulo_ModuloId",
                table: "Pregunta");

            migrationBuilder.AlterColumn<int>(
                name: "ModuloId",
                table: "Pregunta",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "IdModulo",
                table: "Pregunta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Pregunta_Modulo_ModuloId",
                table: "Pregunta",
                column: "ModuloId",
                principalTable: "Modulo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
