using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class compañiaauditora2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CompAuditoras_CompAuditoraId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CompAuditoras_CompAuditoraId",
                table: "AspNetUsers",
                column: "CompAuditoraId",
                principalTable: "CompAuditoras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CompAuditoras_CompAuditoraId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CompAuditoras_CompAuditoraId",
                table: "AspNetUsers",
                column: "CompAuditoraId",
                principalTable: "CompAuditoras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
