﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Ttc.DataAccess.Services;
using Ttc.Model;
using Ttc.Model.Clubs;
using Ttc.Model.Players;
using Ttc.WebApi.Utilities;
using Ttc.WebApi.Utilities.Auth;
using Ttc.WebApi.Utilities.PongRank;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/players")]
public class PlayersController
{
    private readonly PlayerService _service;
    private readonly UserProvider _user;
    private readonly IHubContext<TtcHub, ITtcHub> _hub;
    private readonly PongRankClient _pongRankClient;

    #region Constructor
    public PlayersController(
        PlayerService service,
        UserProvider user,
        IHubContext<TtcHub, ITtcHub> hub,
        PongRankClient pongRankClient)
    {
        _service = service;
        _user = user;
        _hub = hub;
        _pongRankClient = pongRankClient;
    }
    #endregion

    [HttpGet]
    [AllowAnonymous]
    public async Task<CacheResponse<Player>?> Get([FromQuery] DateTime? lastChecked)
    {
        var result = await _service.GetOwnClub(lastChecked);
        if (result == null)
        {
            return null;
        }

        var cleanedResult = _user.CleanSensitiveData(result.Data);
        return new CacheResponse<Player>(cleanedResult, DateTime.MinValue);
    }

    [HttpGet("Events")]
    public async Task<IEnumerable<EventModel>> GetEvents()
    {
        var result = await _service.GetEvents();
        return result;
    }

    [HttpGet("Quitters")]
    public async Task<IEnumerable<Player>> GetQuitters()
    {
        var result = await _service.GetQuitters();
        return result;
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<Player> Get(int id)
    {
        var result = await _service.GetPlayer(id);
        var cleanedResult = _user.CleanSensitiveData(result);
        return cleanedResult;
    }

    [HttpPost]
    [Route("UpdateStyle")]
    public async Task<Player?> UpdateStyle([FromBody] PlayerStyle playerStyle)
    {
        var result = await _service.UpdateStyle(playerStyle);
        if (result != null)
        {
            await _hub.Clients.All.BroadcastReload(Entities.Player, result.Id);
            return result;
        }
        return null;
    }

    [HttpPost]
    [Route("UpdatePlayer")]
    public async Task<Player> UpdatePlayer([FromBody] Player player)
    {
        var result = await _service.UpdatePlayer(player);
        if (result != null)
        {
            await _hub.Clients.All.BroadcastReload(Entities.Player, result.Id);
        }
        return result ?? player;
    }

    [HttpPost]
    [Route("DeletePlayer/{playerId:int}")]
    public async Task DeletePlayer(int playerId)
    {
        await _service.DeletePlayer(playerId);
    }

    [HttpPost]
    [Route("FrenoySync")]
    public async Task FrenoySync()
    {
        await _service.FrenoySync();
    }

    [HttpGet(nameof(GetNextYearRankings))]
    [AllowAnonymous]
    public Task<IEnumerable<PredictionResult>> GetNextYearRankings()
    {
        return _pongRankClient.Get();
    }

    [HttpGet]
    [Route("ExcelExport")]
    public async Task<string> GetExcelExport()
    {
        byte[] excel = await _service.GetExcelExport();
        return Convert.ToBase64String(excel);
    }
}
