using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartOfficeAPP.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex_TicketsIncidents_Statut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_TicketsIncidents_Statut",
                table: "TicketsIncidents",
                column: "Statut");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TicketsIncidents_Statut",
                table: "TicketsIncidents");

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
        }
    }
}
