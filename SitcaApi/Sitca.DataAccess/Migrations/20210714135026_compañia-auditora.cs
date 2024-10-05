using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class compañiaauditora : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompAuditoraId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompAuditoras",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 120, nullable: true),
                    Direccion = table.Column<string>(maxLength: 200, nullable: true),
                    Email = table.Column<string>(maxLength: 120, nullable: true),
                    Telefono = table.Column<string>(maxLength: 20, nullable: true),
                    PaisId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompAuditoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompAuditoras_Pais_PaisId",
                        column: x => x.PaisId,
                        principalTable: "Pais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompAuditoraId",
                table: "AspNetUsers",
                column: "CompAuditoraId");

            migrationBuilder.CreateIndex(
                name: "IX_CompAuditoras_PaisId",
                table: "CompAuditoras",
                column: "PaisId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CompAuditoras_CompAuditoraId",
                table: "AspNetUsers",
                column: "CompAuditoraId",
                principalTable: "CompAuditoras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CompAuditoras_CompAuditoraId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CompAuditoras");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompAuditoraId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompAuditoraId",
                table: "AspNetUsers");
        }
    }
}
