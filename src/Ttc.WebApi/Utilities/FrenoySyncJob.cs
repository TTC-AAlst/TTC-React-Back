using Microsoft.AspNetCore.SignalR;
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
        var hub = scope.ServiceProvider.GetRequiredService<IHubContext<TtcHub, ITtcHub>>();
        try
        {
            logger.Information("FrenoySyncJob Started at {date}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

            await using var context = scope.ServiceProvider.GetRequiredService<ITtcDbContext>();
            var controller = scope.ServiceProvider.GetRequiredService<MatchesController>();

            var matchesToSync = await context.Matches
                .Where(x => !x.IsSyncedWithFrenoy)
                .Where(x => x.ShouldBePlayed)
                .Where(x => !x.WalkOver)
                .Where(x => x.Date < DateTime.Now)
                .Where(x => x.Date != DateTime.MinValue)
                .ToArrayAsync();

            logger.Information("FrenoySyncJob: Syncing {matches} Matches.", matchesToSync.Length);
            bool allSynced = true;
            foreach (var match in matchesToSync)
            {
                bool synced;
                var matchId = new IdDto() { Id = match.Id };
                if (match.AwayTeamId == Constants.OwnClubId || match.HomeClubId == Constants.OwnClubId)
                {
                    Match? syncedMatch = await controller.FrenoyMatchSync(matchId, true);
                    await hub.Clients.All.BroadcastReload(Entities.Match, match.Id);
                    synced = syncedMatch?.IsSyncedWithFrenoy == true;
                }
                else
                {
                    OtherMatch? syncedMatch = await controller.FrenoyOtherMatchSync(matchId, true);
                    await hub.Clients.All.BroadcastReload(Entities.ReadOnlyMatch, match.Id);
                    synced = syncedMatch?.IsSyncedWithFrenoy == true;
                }

                if (synced)
                {
                    logger.Information("FrenoySyncJob: Sync completed for match {matchId}", match.Id);
                }
                else
                {
                    logger.Information("FrenoySyncJob: Partial sync for match {matchId}", match.Id);
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
                logger.Information("FrenoySyncJob: Sync completed for all matches, next sync scheduled for {nextMatchStart}", nextMatchStart);
                _timer?.Change(nextMatchStart, Timeout.InfiniteTimeSpan);
            }
            else
            {
                logger.Information("FrenoySyncJob: Sync still busy, next run in 15min");
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