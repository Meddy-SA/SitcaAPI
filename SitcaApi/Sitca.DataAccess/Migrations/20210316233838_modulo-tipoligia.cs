using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class modulotipoligia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdTipologia",
                table: "Modulo");

            migrationBuilder.AddColumn<int>(
                name: "TipologiaId",
                table: "Modulo",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modulo_TipologiaId",
                table: "Modulo",
                column: "TipologiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modulo_Tipologia_TipologiaId",
                table: "Modulo",
                column: "TipologiaId",
                principalTable: "Tipologia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modulo_Tipologia_TipologiaId",
                table: "Modulo");

            migrationBuilder.DropIndex(
                name: "IX_Modulo_TipologiaId",
                table: "Modulo");

            migrationBuilder.DropColumn(
                name: "TipologiaId",
                table: "Modulo");

            migrationBuilder.AddColumn<int>(
                name: "IdTipologia",
                table: "Modulo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
