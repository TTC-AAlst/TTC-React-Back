using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ttc.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PlayerPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE Player ADD Password NVARCHAR(255) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE Player DROP COLUMN Password;");
        }
    }
}
