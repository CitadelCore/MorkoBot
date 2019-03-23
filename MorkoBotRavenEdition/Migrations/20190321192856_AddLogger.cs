using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MorkoBotRavenEdition.Migrations
{
    public partial class AddLogger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_RestrictionLevel",
                table: "VanityRoles");

            migrationBuilder.AlterColumn<long>(
                name: "Guild",
                table: "VanityRoles",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "VanityRoles",
                nullable: false,
                oldClrType: typeof(decimal))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserWarnings",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<long>(
                name: "StaffId",
                table: "UserWarnings",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<long>(
                name: "GuildIdentifier",
                table: "UserWarnings",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<long>(
                name: "GuildIdentifier",
                table: "UserProfiles",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<long>(
                name: "Identifier",
                table: "UserProfiles",
                nullable: false,
                oldClrType: typeof(decimal))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "UserItems",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<long>(
                name: "Guild",
                table: "UserItems",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<long>(
                name: "Identifier",
                table: "Guilds",
                nullable: false,
                oldClrType: typeof(decimal))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateTable(
                name: "LoggedMessages",
                columns: table => new
                {
                    Identifier = table.Column<long>(nullable: false)
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
                    table.PrimaryKey("PK_LoggedMessages", x => x.Identifier);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoggedMessages");

            migrationBuilder.AlterColumn<decimal>(
                name: "Guild",
                table: "VanityRoles",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "Id",
                table: "VanityRoles",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "_RestrictionLevel",
                table: "VanityRoles",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UserId",
                table: "UserWarnings",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "StaffId",
                table: "UserWarnings",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildIdentifier",
                table: "UserWarnings",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildIdentifier",
                table: "UserProfiles",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "Identifier",
                table: "UserProfiles",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<decimal>(
                name: "UserId",
                table: "UserItems",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "Guild",
                table: "UserItems",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<decimal>(
                name: "Identifier",
                table: "Guilds",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
        }
    }
}
