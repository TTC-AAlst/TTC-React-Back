using Frenoy.Api;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Ttc.DataAccess;

internal static class NewSeasonSeed
{
    /// <summary>
    /// Adds the matches and syncs the players for the new season
    /// </summary>
    public static async Task Seed(ITtcDbContext context, bool clearMatches, int year)
    {
        if (year < 1000)
            throw new Exception("Year should be actual year (ex: 2025), not FrenoySeason (ex: 26)");

        //if (clearMatches)
        //{
        //    context.Database.ExecuteSqlCommand("DELETE FROM matchplayer");
        //    context.Database.ExecuteSqlCommand("DELETE FROM matchgame");
        //    context.Database.ExecuteSqlCommand("DELETE FROM matchcomment");
        //    context.Database.ExecuteSqlCommand("DELETE FROM matches");
        //}

        if (!context.Matches.Any(x => x.FrenoySeason == year))
        {
            // VTTL
            var vttlPlayers = new FrenoyPlayersApi(context, Competition.Vttl);
            // await vttlPlayers.StopAllPlayers(false);
            await vttlPlayers.SyncPlayers();

            var vttl = new FrenoyMatchesApi(context, Competition.Vttl);
            await vttl.SyncTeamsAndMatches();


            // Sporta
            var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
            // await sportaPlayers.StopAllPlayers(false);
            await sportaPlayers.SyncPlayers();

            var sporta = new FrenoyMatchesApi(context, Competition.Sporta);
            await sporta.SyncTeamsAndMatches();

            context.CurrentSeason = year;
        }
    }
}
