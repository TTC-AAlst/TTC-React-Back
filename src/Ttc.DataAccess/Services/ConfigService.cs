using Microsoft.EntityFrameworkCore;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess.Services;

public class ConfigService
{
    private readonly ITtcDbContext _context;

    public ConfigService(ITtcDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, string>> Get()
    {
        var keys = new[]
        {
            "email", "googleMapsUrl", "location", "trainingDays", "competitionDays",
            "adultMembership", "youthMembership", "additionalMembership", "recreationalMembers",
            "frenoyClubIdVttl", "frenoyClubIdSporta", "compBalls", "clubBankNr", "clubOrgNr", "year",
            "endOfSeason"
        };

        var parameters = await _context.Parameters.Where(x => keys.Contains(x.Key)).ToArrayAsync();
        return parameters.ToDictionary(x => x.Key, x => x.Value);
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
            await NewSeasonSeed.Seed(_context, false, newYear);
        }
        else
        {
            param.Value = value;
        }
        await _context.SaveChangesAsync();
    }
}