using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ttc.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MatchGamesDoublePlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayPlayerUniqueIndex2",
                table: "MatchGame",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomePlayerUniqueIndex2",
                table: "MatchGame",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayPlayerUniqueIndex2",
                table: "MatchGame");

            migrationBuilder.DropColumn(
                name: "HomePlayerUniqueIndex2",
                table: "MatchGame");
        }
    }
}
