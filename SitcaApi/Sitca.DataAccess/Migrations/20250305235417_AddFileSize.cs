using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitca.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ProcesoArchivos",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ProcesoArchivos");
        }
    }
}
