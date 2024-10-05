using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class tipologiaenseccionModulo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipologiaId",
                table: "SeccionModulo",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeccionModulo_TipologiaId",
                table: "SeccionModulo",
                column: "TipologiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_SeccionModulo_Tipologia_TipologiaId",
                table: "SeccionModulo",
                column: "TipologiaId",
                principalTable: "Tipologia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeccionModulo_Tipologia_TipologiaId",
                table: "SeccionModulo");

            migrationBuilder.DropIndex(
                name: "IX_SeccionModulo_TipologiaId",
                table: "SeccionModulo");

            migrationBuilder.DropColumn(
                name: "TipologiaId",
                table: "SeccionModulo");
        }
    }
}
