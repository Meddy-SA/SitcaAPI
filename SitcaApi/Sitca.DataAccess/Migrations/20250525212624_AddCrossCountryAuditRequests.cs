using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sitca.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCrossCountryAuditRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CrossCountryAuditRequests",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestingCountryId = table.Column<int>(type: "int", nullable: false),
                    ApprovingCountryId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedAuditorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DeadlineDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotesRequest = table.Column<string>(
                        type: "nvarchar(1000)",
                        maxLength: 1000,
                        nullable: true
                    ),
                    NotesApproval = table.Column<string>(
                        type: "nvarchar(1000)",
                        maxLength: 1000,
                        nullable: true
                    ),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrossCountryAuditRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrossCountryAuditRequests_AspNetUsers_AssignedAuditorId",
                        column: x => x.AssignedAuditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "FK_CrossCountryAuditRequests_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id"
                    );
                    table.ForeignKey(
                        name: "FK_CrossCountryAuditRequests_Pais_ApprovingCountryId",
                        column: x => x.ApprovingCountryId,
                        principalTable: "Pais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_CrossCountryAuditRequests_Pais_RequestingCountryId",
                        column: x => x.RequestingCountryId,
                        principalTable: "Pais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_CrossCountryAuditRequest_ApprovingCountry_Status",
                table: "CrossCountryAuditRequests",
                columns: new[] { "ApprovingCountryId", "Status" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_CrossCountryAuditRequest_RequestingCountry_Status",
                table: "CrossCountryAuditRequests",
                columns: new[] { "RequestingCountryId", "Status" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_CrossCountryAuditRequests_AssignedAuditorId",
                table: "CrossCountryAuditRequests",
                column: "AssignedAuditorId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_CrossCountryAuditRequests_CreatedBy",
                table: "CrossCountryAuditRequests",
                column: "CreatedBy"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CrossCountryAuditRequests");
        }
    }
}
