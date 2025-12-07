using FrenoyVttl;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Core;
using Ttc.Model.Players;

namespace Frenoy.Api;

public class FrenoyClubApi : FrenoyApiBase
{
    private readonly TtcLogger _logger;

    public FrenoyClubApi(ITtcDbContext ttcDbContext, TtcLogger logger, Competition comp) : base(ttcDbContext, comp)
    {
        _logger = logger;
    }

    #region ClubVenues
    public async Task SyncClubVenues()
    {
        Func<ClubEntity, string> getClubCode;
        IEnumerable<ClubEntity> clubs;
        if (_isVttl)
        {
            getClubCode = dbClub => dbClub.CodeVttl!;
            clubs = await _db.Clubs
                .Include(x => x.Locations)
                .Where(club => club.Active)
                .Where(club => club.Id != Constants.OwnClubId)
                .Where(club => !string.IsNullOrEmpty(club.CodeVttl))
                .ToArrayAsync();
        }
        else
        {
            getClubCode = dbClub => dbClub.CodeSporta!;
            clubs = await _db.Clubs
                .Include(x => x.Locations)
                .Where(club => club.Active)
                .Where(club => club.Id != Constants.OwnClubId)
                .Where(club => !string.IsNullOrEmpty(club.CodeSporta))
                .ToArrayAsync();
        }
        await SyncClubVenues(clubs, getClubCode);
    }

    private async Task SyncClubVenues(IEnumerable<ClubEntity> clubs, Func<ClubEntity, string> getClubCode)
    {
        foreach (var dbClub in clubs)
        {
            var oldVenues = await _db.ClubLocations.Where(x => x.ClubId == dbClub.Id).ToArrayAsync();

            GetClubsResponse1? frenoyClubs;

            try
            {
                frenoyClubs = await _frenoy.GetClubsAsync(new GetClubsRequest
                {
                    GetClubs = new GetClubs()
                    {
                        Club = getClubCode(dbClub)
                    }
                });
            }
            catch (Exception ex) when (ex.Message == $"Club [{getClubCode(dbClub)}] is not valid.")
            {
                if (_isVttl)
                {
                    if (string.IsNullOrWhiteSpace(dbClub.CodeSporta))
                    {
                        dbClub.Active = false;
                    }
                    else
                    {
                        dbClub.CodeVttl = null;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(dbClub.CodeVttl))
                    {
                        dbClub.Active = false;
                    }
                    else
                    {
                        dbClub.CodeSporta = null;
                    }
                }
                continue;
            }
            catch (Exception ex)
            {
                var comp = _isVttl ? Competition.Vttl : Competition.Sporta;
                _logger.Error(ex, $"ClubVenueSync: For {comp} ClubId={dbClub.Id} ({dbClub.Name}), Code {getClubCode(dbClub)} crashed", ex.Message);
                continue;
            }

            var frenoyClub = frenoyClubs.GetClubsResponse.ClubEntries.FirstOrDefault();
            if (frenoyClub == null)
            {
                var comp = _isVttl ? Competition.Vttl : Competition.Sporta;
                _logger.Information($"ClubVenueSync: For {comp} ClubId={dbClub.Id} ({dbClub.Name}), Code {getClubCode(dbClub)} is incorrect");
            }
            else if (frenoyClub.VenueEntries == null)
            {
                var comp = _isVttl ? Competition.Vttl : Competition.Sporta;
                _logger.Information($"ClubVenueSync: For {comp} ClubId={dbClub.Id} ({dbClub.Name}), Code {getClubCode(dbClub)} there are no venues");
            }
            else
            {
                _db.ClubLocations.RemoveRange(oldVenues);

                foreach (var frenoyVenue in frenoyClub.VenueEntries)
                {
                    var venue = new ClubLocationEntity
                    {
                        Description = frenoyVenue.Name,
                        Address = frenoyVenue.Street,
                        ClubId = dbClub.Id,
                        City = frenoyVenue.Town.Substring(frenoyVenue.Town.IndexOf(" ") + 1),
                        Mobile = frenoyVenue.Phone,
                        PostalCode = int.Parse(frenoyVenue.Town.Substring(0, frenoyVenue.Town.IndexOf(" "))),
                        MainLocation = frenoyVenue.ClubVenue == "1",
                        Comment = frenoyVenue.Comment
                    };
                    await _db.ClubLocations.AddAsync(venue);
                }
            }
        }
        await _db.SaveChangesAsync();
    }
    #endregion
}
