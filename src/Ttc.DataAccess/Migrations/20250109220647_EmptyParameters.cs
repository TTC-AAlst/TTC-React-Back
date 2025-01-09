using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ttc.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EmptyParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            INSERT INTO `Parameter` (`Key`, `Value`, `Description`) VALUES
	            ('email', 'info@ttc.be', 'Email club'),
	            ('year', '2024', 'Huidig speeljaar'),
	            ('endOfSeason', 'false', NULL),
	            ('frenoy_password', '', 'Paswoord voor synchronisatie met Frenoy'),
	            ('frenoy_login', '', 'Login voor synchronisatie met Frenoy'),
	            ('trainingDays', 'Vrije Training: ma. en woe. tussen 19u30 en 22u30', 'Trainings dagen/tijdstippen'),
	            ('competitionDays', 'Competitie: Maandag en vrijdag om 20u', 'Competitie dagen/tijdstippen'),
	            ('adultMembership', '€125 voor volwassenen (Competitie)', 'Lidgeld voor volwassenen'),
	            ('youthMembership', '€75 voor  jeugdspelers (-18 jarigen)', 'Lidgeld voor -18 jarigen'),
	            ('frenoyClubIdVttl', 'OVL134', 'Frenoy ClubId'),
	            ('SendGridApiKey', '', ''),
	            ('FromEmail', 'info@ttc.be', ''),
	            ('googleMapsUrl', 'https://www.google.com/maps/embed?xxx', NULL),
	            ('location', 'Sportzaal Technigo (""De Voorstad""), 1e verdieping,  Cesar Haeltermansstraat 71, 9300 Aalst', NULL),
	            ('clubBankNr', 'BE55 0016 5927 6744', NULL),
	            ('clubOrgNr', 'BE 0840.545.283', NULL),
	            ('compBalls', 'VICTAS - VP40+ 3 STARS BALLS ', NULL),
	            ('frenoyClubIdSporta', '4055', NULL),
	            ('additionalMembership', '', NULL),
	            ('recreationalMembers', '€75 voor recreanten', NULL);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETER FROM Parameter");
        }
    }
}
