using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitca.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldCuestionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevisionAuditor",
                table: "Cuestionario",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TecnicoPaisId",
                table: "Cuestionario",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NotificacionesEnviadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CertificacionId = table.Column<int>(type: "int", nullable: false),
                    FechaNotificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesEnviadas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificacionesEnviadas");

            migrationBuilder.DropColumn(
                name: "FechaRevisionAuditor",
                table: "Cuestionario");

            migrationBuilder.DropColumn(
                name: "TecnicoPaisId",
                table: "Cuestionario");
        }
    }
}
