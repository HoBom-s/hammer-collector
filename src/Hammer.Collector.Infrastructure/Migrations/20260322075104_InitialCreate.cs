using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hammer.Collector.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gateway_request_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    query_string = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    status_code = table.Column<int>(type: "integer", nullable: false),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false),
                    client_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    request_size = table.Column<long>(type: "bigint", nullable: true),
                    response_size = table.Column<long>(type: "bigint", nullable: true),
                    route_cluster = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gateway_request_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "service_error_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    level = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    exception_type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    stack_trace = table.Column<string>(type: "text", nullable: true),
                    request_path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    request_method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_service_error_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gateway_request_logs_client_ip",
                table: "gateway_request_logs",
                column: "client_ip");

            migrationBuilder.CreateIndex(
                name: "ix_gateway_request_logs_route_cluster",
                table: "gateway_request_logs",
                column: "route_cluster");

            migrationBuilder.CreateIndex(
                name: "ix_gateway_request_logs_timestamp",
                table: "gateway_request_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_gateway_request_logs_trace_id",
                table: "gateway_request_logs",
                column: "trace_id");

            migrationBuilder.CreateIndex(
                name: "ix_service_error_logs_timestamp",
                table: "service_error_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_service_error_logs_trace_id",
                table: "service_error_logs",
                column: "trace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gateway_request_logs");

            migrationBuilder.DropTable(
                name: "service_error_logs");
        }
    }
}
