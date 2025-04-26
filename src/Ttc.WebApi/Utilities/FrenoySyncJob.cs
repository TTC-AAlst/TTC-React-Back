using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Core;
using Ttc.Model.Matches;
using Ttc.WebApi.Controllers;

namespace Ttc.WebApi.Utilities;

public class FrenoySyncJob : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private Timer? _timer;

    public FrenoySyncJob(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(async _ => await SyncMatches(), null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
        return Task.CompletedTask;
    }

    private async Task SyncMatches()
    {
        using var scope = _services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<TtcLogger>();
        try
        {
            logger.Information($"SyncJob Started at {DateTime.Now:dd/MM/yyyy}");

            var context = scope.ServiceProvider.GetRequiredService<ITtcDbContext>();
            var controller = scope.ServiceProvider.GetRequiredService<MatchesController>();

            var matchesToSync = await context.Matches
                .Where(x => !x.IsSyncedWithFrenoy)
                .Where(x => x.ShouldBePlayed)
                .Where(x => !x.WalkOver)
                .Where(x => x.Date < DateTime.Now)
                .Where(x => x.Date != DateTime.MinValue)
                .ToArrayAsync();

            logger.Information($"Syncing {matchesToSync.Length} Matches.");
            bool allSynced = true;
            foreach (var match in matchesToSync)
            {
                bool synced;
                var matchId = new IdDto() { Id = match.Id };
                if (match.AwayTeamId == Constants.OwnClubId || match.HomeClubId == Constants.OwnClubId)
                {
                    Match? syncedMatch = await controller.FrenoyMatchSync(matchId, true);
                    synced = syncedMatch?.IsSyncedWithFrenoy == true;
                }
                else
                {
                    OtherMatch? syncedMatch = await controller.FrenoyOtherMatchSync(matchId, true);
                    synced = syncedMatch?.IsSyncedWithFrenoy == true;
                }

                if (synced)
                {
                    logger.Information($"Sync completed for match {match.Id}");
                }
                else
                {
                    allSynced = false;
                }
            }

            if (allSynced)
            {
                var nextMatch = await context.Matches
                    .Where(x => !x.IsSyncedWithFrenoy)
                    .Where(x => x.Date > DateTime.Now)
                    .OrderBy(x => x.Date)
                    .FirstOrDefaultAsync();

                var nextMatchStart = nextMatch == null ? TimeSpan.FromDays(1) : nextMatch.Date - DateTime.Now;
                _timer?.Change(nextMatchStart, Timeout.InfiniteTimeSpan);
            }
            else
            {
                _timer?.Change(TimeSpan.FromMinutes(15), Timeout.InfiniteTimeSpan);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "FrenoySyncJob failed {Message}", ex.Message);
            _timer?.Change(TimeSpan.FromMinutes(15), Timeout.InfiniteTimeSpan);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}