using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitca.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AbstractEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProcesoCertificacion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ProcesoCertificacion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "ProcesoCertificacion",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProcesoCertificacion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ProcesoCertificacion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PaisId",
                table: "AspNetUsers",
                column: "PaisId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Pais_PaisId",
                table: "AspNetUsers",
                column: "PaisId",
                principalTable: "Pais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Pais_PaisId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PaisId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProcesoCertificacion");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ProcesoCertificacion");
        }
    }
}
