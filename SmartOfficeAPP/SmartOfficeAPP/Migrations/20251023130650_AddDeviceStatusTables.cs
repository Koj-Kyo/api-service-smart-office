using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartOfficeAPP.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceStatusTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IotDeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Rssi = table.Column<int>(type: "integer", nullable: true),
                    HeapFree = table.Column<int>(type: "integer", nullable: true),
                    NvsQueueSize = table.Column<int>(type: "integer", nullable: true),
                    UptimeSeconds = table.Column<long>(type: "bigint", nullable: true),
                    FirmwareVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FirmwareCommit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdditionalData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RfidEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IotDeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CardUid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccessGranted = table.Column<bool>(type: "boolean", nullable: false),
                    DenialReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfidEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmokeAlarmEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IotDeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeactivationReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DeactivatedBy = table.Column<string>(type: "text", nullable: true),
                    ZoneId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmokeAlarmEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmokeAlarmEvents_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMetrics_IotDeviceId_Timestamp",
                table: "DeviceMetrics",
                columns: new[] { "IotDeviceId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RfidEvents_CardUid",
                table: "RfidEvents",
                column: "CardUid");

            migrationBuilder.CreateIndex(
                name: "IX_RfidEvents_IotDeviceId_Timestamp",
                table: "RfidEvents",
                columns: new[] { "IotDeviceId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SmokeAlarmEvents_IotDeviceId_IsActive",
                table: "SmokeAlarmEvents",
                columns: new[] { "IotDeviceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SmokeAlarmEvents_StartTime",
                table: "SmokeAlarmEvents",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_SmokeAlarmEvents_ZoneId",
                table: "SmokeAlarmEvents",
                column: "ZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceMetrics");

            migrationBuilder.DropTable(
                name: "RfidEvents");

            migrationBuilder.DropTable(
                name: "SmokeAlarmEvents");
        }
    }
}
