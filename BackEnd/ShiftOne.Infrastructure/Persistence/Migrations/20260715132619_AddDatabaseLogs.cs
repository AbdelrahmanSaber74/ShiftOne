using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftOne.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RequestMethod = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: true),
                    ExecutionTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    ClientIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ApplicationName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EnvironmentName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_CorrelationId",
                table: "Logs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Level",
                table: "Logs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_RequestPath",
                table: "Logs",
                column: "RequestPath");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_Timestamp",
                table: "Logs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");
        }
    }
}
