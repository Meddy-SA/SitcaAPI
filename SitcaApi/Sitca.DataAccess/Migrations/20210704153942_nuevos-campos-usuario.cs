using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
    public partial class nuevoscamposusuario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "AspNetUsers",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "AspNetUsers",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "AspNetUsers",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoAcreditacion",
                table: "AspNetUsers",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaIngreso",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HojaDeVida",
                table: "AspNetUsers",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCarnet",
                table: "AspNetUsers",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "AspNetUsers",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DocumentoAcreditacion",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FechaIngreso",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HojaDeVida",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NumeroCarnet",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "AspNetUsers");
        }
    }
}
