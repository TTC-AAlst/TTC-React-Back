using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ttc.DataAccess.Services;
using Ttc.DataEntities;
using Ttc.Model.Clubs;
using Ttc.WebApi.Utilities;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/clubs")]
public class ClubsController
{
    #region Constructor
    private readonly ClubService _service;
    private readonly TtcHub _hub;

    public ClubsController(ClubService service, TtcHub hub)
    {
        _service = service;
        _hub = hub;
    }
    #endregion

    [HttpGet]
    [AllowAnonymous]
    public async Task<IEnumerable<Club>> Get() => await _service.GetActiveClubs();

    [HttpPost]
    [Route("UpdateClub")]
    public async Task<Club> UpdateClub([FromBody] Club club)
    {
        var result = await _service.UpdateClub(club);
        await _hub.BroadcastReload(Entities.Club, result.Id);
        return result;
    }

    #region Club Board
    [HttpPost]
    [Route("Board")]
    public async Task SaveBoardMember([FromBody] BoardMember m)
    {
        await _service.SaveBoardMember(m.PlayerId, m.BoardFunction, m.Sort);
        await _hub.BroadcastReload(Entities.Player, Constants.OwnClubId);
    }

    [HttpPost]
    [Route("Board/{playerId:int}")]
    public async Task DeleteBoardMember(int playerId)
    {
        await _service.DeleteBoardMember(playerId);
        await _hub.BroadcastReload(Entities.Player, Constants.OwnClubId);
    }

    public class BoardMember
    {
        public int PlayerId { get; set; }
        public string BoardFunction { get; set; } = "";
        public int Sort { get; set; }

        public override string ToString() => $"{PlayerId} => {BoardFunction} ({Sort})";
    }
    #endregion
}
