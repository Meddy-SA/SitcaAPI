using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class camposcontactoempresa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Empresa",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebSite",
                table: "Empresa",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Empresa");

            migrationBuilder.DropColumn(
                name: "WebSite",
                table: "Empresa");
        }
    }
}
