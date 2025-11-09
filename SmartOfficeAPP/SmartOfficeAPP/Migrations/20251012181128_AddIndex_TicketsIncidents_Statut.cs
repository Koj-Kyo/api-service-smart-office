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
            // ✅ Vérifier et supprimer les colonnes SEULEMENT si elles existent
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'TicketsIncidents' AND column_name = 'AssignedToId') THEN
                        ALTER TABLE ""TicketsIncidents"" DROP COLUMN ""AssignedToId"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'TicketsIncidents' AND column_name = 'CreatedAt') THEN
                        ALTER TABLE ""TicketsIncidents"" DROP COLUMN ""CreatedAt"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'TicketsIncidents' AND column_name = 'CreatedById') THEN
                        ALTER TABLE ""TicketsIncidents"" DROP COLUMN ""CreatedById"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'TicketsIncidents' AND column_name = 'Priority') THEN
                        ALTER TABLE ""TicketsIncidents"" DROP COLUMN ""Priority"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'TicketsIncidents' AND column_name = 'Status') THEN
                        ALTER TABLE ""TicketsIncidents"" DROP COLUMN ""Status"";
                    END IF;
                    
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'TicketsIncidents' AND column_name = 'Title') THEN
                        ALTER TABLE ""TicketsIncidents"" DROP COLUMN ""Title"";
                    END IF;
                END $$;
            ");

            // ✅ Créer l'index seulement s'il n'existe pas
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_TicketsIncidents_Statut"" 
                ON ""TicketsIncidents"" (""Statut"");
            ");
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
