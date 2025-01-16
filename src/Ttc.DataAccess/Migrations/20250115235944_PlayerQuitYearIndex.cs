using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ttc.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PlayerQuitYearIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Player_QuitYear",
                table: "Player",
                column: "QuitYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Player_QuitYear",
                table: "Player");
        }
    }
}
