using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ttc.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MatchPlayerIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayer_Player_PlayerId",
                table: "MatchPlayer");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "MatchPlayer",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayer_Player_PlayerId",
                table: "MatchPlayer",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayer_Player_PlayerId",
                table: "MatchPlayer");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "MatchPlayer",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayer_Player_PlayerId",
                table: "MatchPlayer",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
