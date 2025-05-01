using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Core;
using Ttc.Model.Players;

namespace Ttc.WebApi.Utilities.PongRank;

public class PongRankClient
{
    private readonly TtcSettings _settings;
    private readonly ITtcDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly CacheHelper _cache;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PongRankClient(TtcSettings settings, ITtcDbContext db, HttpClient httpClient, IMemoryCache cache)
    {
        _settings = settings;
        _db = db;
        _httpClient = httpClient;
        _cache = new CacheHelper(cache);
    }

    public async Task<IEnumerable<PredictionResult>> Get()
    {
        var result = await _cache.GetOrSet("ranking-predictions", FetchRankings, TimeSpan.FromHours(10));
        return result;
    }

    private async Task<IEnumerable<PredictionResult>> FetchRankings()
    {
        var club = await _db.Clubs.SingleAsync(x => x.Id == Constants.OwnClubId);

        var result = new List<PredictionResult>();
        if (!string.IsNullOrWhiteSpace(club.CodeSporta))
        {
            var sporta = await PredictionResults(Competition.Sporta, club.CodeSporta);
            result.AddRange(sporta);
        }
        if (!string.IsNullOrWhiteSpace(club.CodeVttl))
        {
            var vttl = await PredictionResults(Competition.Vttl, club.CodeVttl);
            result.AddRange(vttl);
        }
        return result.AsEnumerable();
    }

    private async Task<IEnumerable<PredictionResult>> PredictionResults(Competition competition, string clubId)
    {
        string url = $"/Prediction?Competition={competition}&Year={_db.CurrentSeason}&ClubUniqueIndex={clubId}";
        var response = await _httpClient.GetAsync(_settings.PongRankUrl + url);
        var stream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<IEnumerable<PredictionResult>>(stream, JsonOptions);
        if (result == null)
            return [];

        var list = result.ToList();
        list.ForEach(x => x.Competition = competition);
        return list;
    }
}