using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class paisempresa2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaisId",
                table: "Empresa",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresa_PaisId",
                table: "Empresa",
                column: "PaisId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empresa_Pais_PaisId",
                table: "Empresa",
                column: "PaisId",
                principalTable: "Pais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empresa_Pais_PaisId",
                table: "Empresa");

            migrationBuilder.DropIndex(
                name: "IX_Empresa_PaisId",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "PaisId",
                table: "Empresa");
        }
    }
}
