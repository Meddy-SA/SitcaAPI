using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class cumplimientoxtipologia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipologiaId",
                table: "Cumplimiento",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaFinalizado",
                table: "Cuestionario",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVisita",
                table: "Cuestionario",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cumplimiento_TipologiaId",
                table: "Cumplimiento",
                column: "TipologiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cumplimiento_Tipologia_TipologiaId",
                table: "Cumplimiento",
                column: "TipologiaId",
                principalTable: "Tipologia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cumplimiento_Tipologia_TipologiaId",
                table: "Cumplimiento");

            migrationBuilder.DropIndex(
                name: "IX_Cumplimiento_TipologiaId",
                table: "Cumplimiento");

            migrationBuilder.DropColumn(
                name: "TipologiaId",
                table: "Cumplimiento");

            migrationBuilder.DropColumn(
                name: "FechaVisita",
                table: "Cuestionario");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaFinalizado",
                table: "Cuestionario",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }
    }
}
