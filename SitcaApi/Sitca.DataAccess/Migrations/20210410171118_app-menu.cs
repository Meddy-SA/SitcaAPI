using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class appmenu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMenu",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Parent_MenuID = table.Column<int>(nullable: true),
                    Order = table.Column<int>(nullable: true),
                    MenuName = table.Column<string>(maxLength: 30, nullable: true),
                    MenuNameEn = table.Column<string>(maxLength: 30, nullable: true),
                    Roles = table.Column<string>(maxLength: 300, nullable: true),
                    Icon = table.Column<string>(maxLength: 100, nullable: true),
                    IconIsImage = table.Column<bool>(nullable: false),
                    MenuURL = table.Column<string>(maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMenu", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMenu");
        }
    }
}
