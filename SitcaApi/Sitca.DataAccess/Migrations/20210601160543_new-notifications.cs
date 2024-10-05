using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class newnotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotifiyUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Notificacion");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Notificacion",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TextoInterno",
                table: "Notificacion",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextoParaEmpresa",
                table: "Notificacion",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloInterno",
                table: "Notificacion",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloParaEmpresa",
                table: "Notificacion",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificationCustomUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(maxLength: 50, nullable: true),
                    Name = table.Column<string>(maxLength: 150, nullable: true),
                    PaisId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationCustomUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationId = table.Column<int>(nullable: false),
                    RoleId = table.Column<string>(maxLength: 60, nullable: true),
                    NotificacionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationGroups_Notificacion_NotificacionId",
                        column: x => x.NotificacionId,
                        principalTable: "Notificacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomsToNotificate",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomId = table.Column<int>(nullable: false),
                    NotificacionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsToNotificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomsToNotificate_NotificationCustomUsers_CustomId",
                        column: x => x.CustomId,
                        principalTable: "NotificationCustomUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomsToNotificate_Notificacion_NotificacionId",
                        column: x => x.NotificacionId,
                        principalTable: "Notificacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomsToNotificate_CustomId",
                table: "CustomsToNotificate",
                column: "CustomId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomsToNotificate_NotificacionId",
                table: "CustomsToNotificate",
                column: "NotificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationGroups_NotificacionId",
                table: "NotificationGroups",
                column: "NotificacionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomsToNotificate");

            migrationBuilder.DropTable(
                name: "NotificationGroups");

            migrationBuilder.DropTable(
                name: "NotificationCustomUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "TextoInterno",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "TextoParaEmpresa",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "TituloInterno",
                table: "Notificacion");

            migrationBuilder.DropColumn(
                name: "TituloParaEmpresa",
                table: "Notificacion");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Notificacion",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotifiyUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
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
    }
}
