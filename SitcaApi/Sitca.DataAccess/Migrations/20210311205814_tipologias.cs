﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Sitca.DataAccess.Migrations
{
  public partial class Tipologias : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
          name: "Name",
          table: "AspNetUserTokens",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "nvarchar(128)",
          oldMaxLength: 128);

      migrationBuilder.AlterColumn<string>(
          name: "LoginProvider",
          table: "AspNetUserTokens",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "nvarchar(128)",
          oldMaxLength: 128);

      migrationBuilder.AlterColumn<string>(
          name: "ProviderKey",
          table: "AspNetUserLogins",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "nvarchar(128)",
          oldMaxLength: 128);

      migrationBuilder.AlterColumn<string>(
          name: "LoginProvider",
          table: "AspNetUserLogins",
          nullable: false,
          oldClrType: typeof(string),
          oldType: "nvarchar(128)",
          oldMaxLength: 128);

      migrationBuilder.CreateTable(
          name: "Tipologias",
          columns: table => new
          {
            Id = table.Column<int>(nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(maxLength: 75, nullable: true),
            active = table.Column<bool>(nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Tipologias", x => x.Id);
          });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Tipologias");

      migrationBuilder.AlterColumn<string>(
          name: "Name",
          table: "AspNetUserTokens",
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: false,
          oldClrType: typeof(string));

      migrationBuilder.AlterColumn<string>(
          name: "LoginProvider",
          table: "AspNetUserTokens",
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: false,
          oldClrType: typeof(string));

      migrationBuilder.AlterColumn<string>(
          name: "ProviderKey",
          table: "AspNetUserLogins",
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: false,
          oldClrType: typeof(string));

      migrationBuilder.AlterColumn<string>(
          name: "LoginProvider",
          table: "AspNetUserLogins",
          type: "nvarchar(128)",
          maxLength: 128,
          nullable: false,
          oldClrType: typeof(string));
    }
  }
}
