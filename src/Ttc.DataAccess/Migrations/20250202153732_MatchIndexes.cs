using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ttc.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MatchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Match_FrenoyMatchId",
                table: "Match",
                column: "FrenoyMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_FrenoySeason_AwayClubId",
                table: "Match",
                columns: new[] { "FrenoySeason", "AwayClubId" });

            migrationBuilder.CreateIndex(
                name: "IX_Match_FrenoySeason_HomeClubId",
                table: "Match",
                columns: new[] { "FrenoySeason", "HomeClubId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Match_FrenoyMatchId",
                table: "Match");

            migrationBuilder.DropIndex(
                name: "IX_Match_FrenoySeason_AwayClubId",
                table: "Match");

            migrationBuilder.DropIndex(
                name: "IX_Match_FrenoySeason_HomeClubId",
                table: "Match");
        }
    }
}
