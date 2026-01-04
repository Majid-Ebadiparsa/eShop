using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthMonitorService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceHealthHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsHealthy = table.Column<bool>(type: "bit", nullable: false),
                    StatusMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHealthHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceHealthStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsHealthy = table.Column<bool>(type: "bit", nullable: false),
                    StatusMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StackTrace = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHealthStatuses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHealthHistories_ServiceName_CheckedAt",
                table: "ServiceHealthHistories",
                columns: new[] { "ServiceName", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHealthStatuses_ServiceName",
                table: "ServiceHealthStatuses",
                column: "ServiceName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceHealthHistories");

            migrationBuilder.DropTable(
                name: "ServiceHealthStatuses");
        }
    }
}
