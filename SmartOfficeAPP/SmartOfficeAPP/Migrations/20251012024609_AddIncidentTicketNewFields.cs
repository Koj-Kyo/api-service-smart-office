using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOfficeAPP.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentTicketNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Telemetries_PeripheriqueLogiqueId",
                table: "Telemetries");

            migrationBuilder.CreateIndex(
                name: "IX_Telemetries_PeripheriqueLogiqueId_Horodatage",
                table: "Telemetries",
                columns: new[] { "PeripheriqueLogiqueId", "Horodatage" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Telemetries_PeripheriqueLogiqueId_Horodatage",
                table: "Telemetries");

            migrationBuilder.CreateIndex(
                name: "IX_Telemetries_PeripheriqueLogiqueId",
                table: "Telemetries",
                column: "PeripheriqueLogiqueId");
        }
    }
}
