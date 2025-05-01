using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PongRankClient(TtcSettings settings, ITtcDbContext db, HttpClient httpClient)
    {
        _settings = settings;
        _db = db;
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<PredictionResult>> Get()
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
        return result;
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