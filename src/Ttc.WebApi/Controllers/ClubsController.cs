using Frenoy.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Ttc.DataAccess.Services;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Clubs;
using Ttc.Model.Core;
using Ttc.Model.Players;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/clubs")]
public class ClubsController
{
    #region Constructor
    private readonly ClubService _service;
    private readonly IHubContext<TtcHub, ITtcHub> _hub;
    private readonly ITtcDbContext _dbContext;
    private readonly TtcLogger _logger;

    public ClubsController(ClubService service, IHubContext<TtcHub, ITtcHub> hub, ITtcDbContext dbContext, TtcLogger logger)
    {
        _service = service;
        _hub = hub;
        _dbContext = dbContext;
        _logger = logger;
    }
    #endregion

    [HttpGet]
    [AllowAnonymous]
    public async Task<CacheResponse<Club>?> Get([FromQuery] DateTime? lastChecked)
    {
        var clubs = await _service.GetActiveClubs(lastChecked);
        return clubs;
    }

    [HttpPost]
    [Route("UpdateClub")]
    public async Task<Club> UpdateClub([FromBody] Club club)
    {
        var result = await _service.UpdateClub(club);
        await _hub.Clients.All.BroadcastReload(Entities.Club, result.Id);
        return result;
    }

    #region Club Board
    [HttpPost]
    [Route("Board")]
    public async Task SaveBoardMember([FromBody] BoardMember m)
    {
        await _service.SaveBoardMember(m.PlayerId, m.BoardFunction, m.Sort);
        await _hub.Clients.All.BroadcastReload(Entities.Club, Constants.OwnClubId);
    }

    [HttpPost]
    [Route("Board/{playerId:int}")]
    public async Task DeleteBoardMember(int playerId)
    {
        await _service.DeleteBoardMember(playerId);
        await _hub.Clients.All.BroadcastReload(Entities.Club, Constants.OwnClubId);
    }

    public class BoardMember
    {
        public int PlayerId { get; set; }
        public string BoardFunction { get; set; } = "";
        public int Sort { get; set; }

        public override string ToString() => $"{PlayerId} => {BoardFunction} ({Sort})";
    }
    #endregion

    [HttpPost]
    [Route("Sync")]
    public async Task Sync()
    {
        var sportaApi = new FrenoyClubApi(_dbContext, _logger, Competition.Sporta);
        await sportaApi.SyncClubVenues();

        var vttlApi = new FrenoyClubApi(_dbContext, _logger, Competition.Vttl);
        await vttlApi.SyncClubVenues();
    }
}
