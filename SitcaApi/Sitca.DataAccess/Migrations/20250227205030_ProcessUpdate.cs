using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitca.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ProcessUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcesoCertificacion_Empresa_EmpresaId",
                table: "ProcesoCertificacion");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ProcesoCertificacion",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Recertificacion",
                table: "ProcesoCertificacion",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "ProcesoCertificacion",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ProcesoCertificacion",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "ProcesoCertificacion",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProcesoArchivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FileTypesCompany = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ProcesoCertificacionId = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesoArchivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcesoArchivos_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcesoArchivos_ProcesoCertificacion_ProcesoCertificacionId",
                        column: x => x.ProcesoCertificacionId,
                        principalTable: "ProcesoCertificacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoCertificacion_CreatedBy",
                table: "ProcesoCertificacion",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoArchivos_CreatedBy",
                table: "ProcesoArchivos",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesoArchivos_ProcesoCertificacionId",
                table: "ProcesoArchivos",
                column: "ProcesoCertificacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesoCertificacion_AspNetUsers_CreatedBy",
                table: "ProcesoCertificacion",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesoCertificacion_Empresa_EmpresaId",
                table: "ProcesoCertificacion",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcesoCertificacion_AspNetUsers_CreatedBy",
                table: "ProcesoCertificacion");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcesoCertificacion_Empresa_EmpresaId",
                table: "ProcesoCertificacion");

            migrationBuilder.DropTable(
                name: "ProcesoArchivos");

            migrationBuilder.DropIndex(
                name: "IX_ProcesoCertificacion_CreatedBy",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "ProcesoCertificacion");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "ProcesoCertificacion",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Recertificacion",
                table: "ProcesoCertificacion",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                table: "ProcesoCertificacion",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ProcesoCertificacion",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcesoCertificacion_Empresa_EmpresaId",
                table: "ProcesoCertificacion",
                column: "EmpresaId",
                principalTable: "Empresa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
