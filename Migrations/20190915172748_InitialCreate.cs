using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MorkoBotRavenEdition.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Identifier = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IncrementCount = table.Column<int>(nullable: false),
                    IncrementTarget = table.Column<int>(nullable: false),
                    DefaultChannel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "InfraCompetitionEntries",
                columns: table => new
                {
                    Identifier = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(nullable: false),
                    Entered = table.Column<DateTime>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    ImageUrl = table.Column<string>(nullable: true),
                    Votes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfraCompetitionEntries", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "LoggedMessages",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guild = table.Column<long>(nullable: false),
                    Author = table.Column<long>(nullable: false),
                    Channel = table.Column<long>(nullable: false),
                    Message = table.Column<long>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    OriginalId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggedMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Identifier = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GuildIdentifier = table.Column<long>(nullable: false),
                    OpenSewerTokens = table.Column<int>(nullable: false),
                    Experience = table.Column<int>(nullable: false),
                    ExperienceTarget = table.Column<int>(nullable: false),
                    ExperienceLevels = table.Column<int>(nullable: false),
                    IncrementCount = table.Column<int>(nullable: false),
                    LastIncremented = table.Column<DateTime>(nullable: false),
                    Health = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "UserWarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GuildIdentifier = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false),
                    StaffId = table.Column<long>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    TimeAdded = table.Column<DateTime>(nullable: false),
                    DaysExpiry = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWarnings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VanityRoles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Guild = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VanityRoles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "InfraCompetitionEntries");

            migrationBuilder.DropTable(
                name: "LoggedMessages");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "UserWarnings");

            migrationBuilder.DropTable(
                name: "VanityRoles");
        }
    }
}
