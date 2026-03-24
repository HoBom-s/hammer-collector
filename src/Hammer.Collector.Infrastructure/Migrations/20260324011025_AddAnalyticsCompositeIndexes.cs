using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hammer.Collector.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_service_error_logs_timestamp_exception_type",
                table: "service_error_logs",
                columns: new[] { "timestamp", "exception_type" });

            migrationBuilder.CreateIndex(
                name: "ix_service_error_logs_timestamp_source",
                table: "service_error_logs",
                columns: new[] { "timestamp", "source" });

            migrationBuilder.CreateIndex(
                name: "ix_gateway_request_logs_timestamp_method_path",
                table: "gateway_request_logs",
                columns: new[] { "timestamp", "method", "path" });

            migrationBuilder.CreateIndex(
                name: "ix_gateway_request_logs_timestamp_status_code",
                table: "gateway_request_logs",
                columns: new[] { "timestamp", "status_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_service_error_logs_timestamp_exception_type",
                table: "service_error_logs");

            migrationBuilder.DropIndex(
                name: "ix_service_error_logs_timestamp_source",
                table: "service_error_logs");

            migrationBuilder.DropIndex(
                name: "ix_gateway_request_logs_timestamp_method_path",
                table: "gateway_request_logs");

            migrationBuilder.DropIndex(
                name: "ix_gateway_request_logs_timestamp_status_code",
                table: "gateway_request_logs");
        }
    }
}
