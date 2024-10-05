using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class notificationsusers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificacion",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 150, nullable: true),
                    Pais = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotifiyUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificacionId = table.Column<int>(nullable: false),
                    UsuarioId = table.Column<string>(maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifiyUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotifiyUsers_Notificacion_NotificacionId",
                        column: x => x.NotificacionId,
                        principalTable: "Notificacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotifiyUsers_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotifiyUsers_NotificacionId",
                table: "NotifiyUsers",
                column: "NotificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotifiyUsers_UsuarioId",
                table: "NotifiyUsers",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotifiyUsers");

            migrationBuilder.DropTable(
                name: "Notificacion");
        }
    }
}
