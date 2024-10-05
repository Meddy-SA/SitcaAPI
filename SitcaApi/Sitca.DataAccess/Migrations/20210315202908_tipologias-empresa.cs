using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class tipologiasempresa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipologiasEmpresa",
                columns: table => new
                {
                    IdEmpresa = table.Column<int>(nullable: false),
                    IdTipologia = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipologiasEmpresa", x => new { x.IdTipologia, x.IdEmpresa });
                    table.ForeignKey(
                        name: "FK_TipologiasEmpresa_Empresa_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TipologiasEmpresa_Tipologia_IdTipologia",
                        column: x => x.IdTipologia,
                        principalTable: "Tipologia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TipologiasEmpresa_IdEmpresa",
                table: "TipologiasEmpresa",
                column: "IdEmpresa");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TipologiasEmpresa");
        }
    }
}
