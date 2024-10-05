using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class preguntatipologia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipologiaId",
                table: "Pregunta",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Modulo",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Pregunta_TipologiaId",
                table: "Pregunta",
                column: "TipologiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pregunta_Tipologia_TipologiaId",
                table: "Pregunta",
                column: "TipologiaId",
                principalTable: "Tipologia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pregunta_Tipologia_TipologiaId",
                table: "Pregunta");

            migrationBuilder.DropIndex(
                name: "IX_Pregunta_TipologiaId",
                table: "Pregunta");

            migrationBuilder.DropColumn(
                name: "TipologiaId",
                table: "Pregunta");

            migrationBuilder.AlterColumn<int>(
                name: "Nombre",
                table: "Modulo",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
