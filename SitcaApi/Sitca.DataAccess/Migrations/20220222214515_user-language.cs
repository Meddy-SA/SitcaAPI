using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class userlanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Lenguage",
                table: "AspNetUsers",
                maxLength: 3,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lenguage",
                table: "AspNetUsers");
        }
    }
}
