using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class fixnotificationgroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationGroups_Notificacion_NotificacionId",
                table: "NotificationGroups");

            migrationBuilder.DropIndex(
                name: "IX_NotificationGroups_NotificacionId",
                table: "NotificationGroups");

            migrationBuilder.DropColumn(
                name: "NotificacionId",
                table: "NotificationGroups");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationGroups_NotificationId",
                table: "NotificationGroups",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationGroups_Notificacion_NotificationId",
                table: "NotificationGroups",
                column: "NotificationId",
                principalTable: "Notificacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationGroups_Notificacion_NotificationId",
                table: "NotificationGroups");

            migrationBuilder.DropIndex(
                name: "IX_NotificationGroups_NotificationId",
                table: "NotificationGroups");

            migrationBuilder.AddColumn<int>(
                name: "NotificacionId",
                table: "NotificationGroups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationGroups_NotificacionId",
                table: "NotificationGroups",
                column: "NotificacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationGroups_Notificacion_NotificacionId",
                table: "NotificationGroups",
                column: "NotificacionId",
                principalTable: "Notificacion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
