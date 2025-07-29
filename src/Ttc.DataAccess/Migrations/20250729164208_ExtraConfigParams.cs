using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ttc.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ExtraConfigParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Parameter (`Key`, Value) VALUES ('trainingDays2', 'Geleide Training: 2 wekelijks woensdag 20u30 tot 22u')");
            migrationBuilder.Sql("INSERT INTO Parameter (`Key`, Value) VALUES ('trainingDays3', 'Jeugd Training: Elke maandag van 18u15 tot 19u15')");
            migrationBuilder.Sql("INSERT INTO Parameter (`Key`, Value) VALUES ('trainingDays4', 'Jeugdwerking vanaf 8 jaar')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
