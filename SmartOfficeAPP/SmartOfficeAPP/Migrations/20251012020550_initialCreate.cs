using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOfficeAPP.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeDonnee",
                table: "Telemetries",
                newName: "TypeEvenement");

            migrationBuilder.AddColumn<string>(
                name: "AssignedToId",
                table: "TicketsIncidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TicketsIncidents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "TicketsIncidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "TicketsIncidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TicketsIncidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TicketsIncidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "Valeur",
                table: "Telemetries",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "DonneesJson",
                table: "Telemetries",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "TicketsIncidents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TicketsIncidents");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "TicketsIncidents");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TicketsIncidents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TicketsIncidents");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TicketsIncidents");

            migrationBuilder.DropColumn(
                name: "DonneesJson",
                table: "Telemetries");

            migrationBuilder.RenameColumn(
                name: "TypeEvenement",
                table: "Telemetries",
                newName: "TypeDonnee");

            migrationBuilder.AlterColumn<decimal>(
                name: "Valeur",
                table: "Telemetries",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
