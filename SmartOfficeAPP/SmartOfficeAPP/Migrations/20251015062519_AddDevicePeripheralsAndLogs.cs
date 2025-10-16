using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartOfficeAPP.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicePeripheralsAndLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevicePeripherals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Gpio = table.Column<int>(type: "integer", nullable: false),
                    IotDeviceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    WiringDiagram = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InstalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastKnownState = table.Column<string>(type: "text", nullable: true),
                    LastUpdateReceived = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PeripheriqueLogiqueId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePeripherals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevicePeripherals_PeripheriquesLogiques_PeripheriqueLogique~",
                        column: x => x.PeripheriqueLogiqueId,
                        principalTable: "PeripheriquesLogiques",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevicePeripherals_IotDeviceId_Gpio",
                table: "DevicePeripherals",
                columns: new[] { "IotDeviceId", "Gpio" });

            migrationBuilder.CreateIndex(
                name: "IX_DevicePeripherals_IotDeviceId_Name",
                table: "DevicePeripherals",
                columns: new[] { "IotDeviceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DevicePeripherals_PeripheriqueLogiqueId",
                table: "DevicePeripherals",
                column: "PeripheriqueLogiqueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevicePeripherals");
        }
    }
}
