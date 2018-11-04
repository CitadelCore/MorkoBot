using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MorkoBotRavenEdition.Migrations
{
    public partial class VanityRoles2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_VanityRoles",
                table: "Guilds");

            migrationBuilder.CreateTable(
                name: "VanityRoles",
                columns: table => new
                {
                    Id = table.Column<decimal>(nullable: false),
                    Guild = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    _RestrictionLevel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanityRoles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VanityRoles");

            migrationBuilder.AddColumn<string>(
                name: "_VanityRoles",
                table: "Guilds",
                nullable: true);
        }
    }
}
