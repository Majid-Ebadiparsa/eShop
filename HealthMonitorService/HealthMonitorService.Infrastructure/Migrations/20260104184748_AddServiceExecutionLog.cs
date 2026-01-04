using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthMonitorService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceExecutionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceExecutionLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExecutionStartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    ExecutionSucceeded = table.Column<bool>(type: "bit", nullable: false),
                    ServiceIsHealthy = table.Column<bool>(type: "bit", nullable: true),
                    ServiceResponseTimeMs = table.Column<long>(type: "bigint", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ServiceAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ServicePort = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceExecutionLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutionLogs_ExecutionStartedAt",
                table: "ServiceExecutionLogs",
                column: "ExecutionStartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceExecutionLogs_ServiceName_ExecutionStartedAt",
                table: "ServiceExecutionLogs",
                columns: new[] { "ServiceName", "ExecutionStartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceExecutionLogs");
        }
    }
}
