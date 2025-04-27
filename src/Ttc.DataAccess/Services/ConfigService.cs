using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess.Services;

public class ConfigService
{
    private readonly ITtcDbContext _context;
    private readonly CacheHelper _cache;

    private static readonly string[] PublicParameters = new[]
    {
        "email", "googleMapsUrl", "location", "trainingDays", "competitionDays",
        "adultMembership", "youthMembership", "additionalMembership", "recreationalMembers",
        "frenoyClubIdVttl", "frenoyClubIdSporta", "compBalls", "clubBankNr", "clubOrgNr", "year",
        "endOfSeason"
    };

    public ConfigService(ITtcDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = new CacheHelper(cache);
    }

    public async Task<Dictionary<string, string>?> Get(DateTime? lastChecked)
    {
        var dict = await _cache.GetOrSet("config", Get, TimeSpan.FromHours(5));
        //DateTime lastCachedChange = DateTime.ParseExact(dict[nameof(Audit.ModifiedOn)], "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        //if (lastChecked.HasValue && lastChecked.Value >= lastCachedChange)
        //{
        //    return null;
        //}
        return dict;
    }

    private async Task<Dictionary<string, string>> Get()
    {
        var parameters = await _context.Parameters.Where(x => PublicParameters.Contains(x.Key)).ToArrayAsync();
        var dict = parameters.ToDictionary(x => x.Key, x => x.Value);
        var lastChange = parameters.Max(x => x.Audit.ModifiedOn ?? DateTime.MinValue);
        dict.Add(nameof(Audit.ModifiedOn), lastChange.ToString("yyyy-MM-ddTHH:mm:ss"));
        return dict;
    }

    public async Task<EmailConfig> GetEmailConfig()
    {
        var sendGridApiKey = (await _context.Parameters.SingleAsync(x => x.Key == "SendGridApiKey")).Value;
        var fromEmail = (await _context.Parameters.SingleAsync(x => x.Key == "FromEmail")).Value;
        return new EmailConfig(fromEmail, sendGridApiKey);
    }

    public async Task Save(string key, string value)
    {
        var param = await _context.Parameters.SingleAsync(x => x.Key == key);
        if (key == "year")
        {
            int newYear = int.Parse(value);
            param.Value = newYear.ToString();
            await _context.SaveChangesAsync();

            await NewSeasonSeed.Seed(_context, false, newYear);

            _cache.Remove("clubs");
            _cache.Remove("players");
            _cache.Remove("teams");
        }
        else
        {
            param.Value = value;
        }
        await _context.SaveChangesAsync();
        _cache.Remove("config");
    }
}