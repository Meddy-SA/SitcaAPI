using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class compauditoraspecial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Special",
                table: "CompAuditoras",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Special",
                table: "CompAuditoras");
        }
    }
}
