using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MorkoBotRavenEdition.Migrations
{
    public partial class IncrementFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GuildIdentifier",
                table: "UserProfiles",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "IncrementCount",
                table: "UserProfiles",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastIncremented",
                table: "UserProfiles",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Identifier = table.Column<decimal>(nullable: false),
                    IncrementCount = table.Column<int>(nullable: false),
                    IncrementTarget = table.Column<int>(nullable: false),
                    DefaultChannel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Identifier);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropColumn(
                name: "GuildIdentifier",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IncrementCount",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LastIncremented",
                table: "UserProfiles");
        }
    }
}
