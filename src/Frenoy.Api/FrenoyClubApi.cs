using System.Diagnostics;
using FrenoyVttl;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Frenoy.Api;

public class FrenoyClubApi : FrenoyApiBase
{
    public FrenoyClubApi(ITtcDbContext ttcDbContext, Competition comp) : base(ttcDbContext, comp)
    {
    }

    #region ClubLokalen
    public async Task SyncClubLokalen()
    {
        // TODO: these methods need to be applied to vttl and sporta together
        // TODO: need to check if frenoy club locations are actually better than current data...

        Debug.Assert(false, "legacy db data might be better?");

        Func<ClubEntity, string> getClubCode;
        IEnumerable<ClubEntity> clubs;
        if (_isVttl)
        {
            getClubCode = dbClub => dbClub.CodeVttl;
            clubs = _db.Clubs.Include(x => x.Locations).Where(club => !string.IsNullOrEmpty(club.CodeVttl)).ToArray();
        }
        else
        {
            getClubCode = dbClub => dbClub.CodeSporta;
            clubs = _db.Clubs.Include(x => x.Locations).Where(club => !string.IsNullOrEmpty(club.CodeSporta)).ToArray();
        }
        await SyncClubLokalen(clubs, getClubCode);
    }

    private async Task SyncClubLokalen(IEnumerable<ClubEntity> clubs, Func<ClubEntity, string> getClubCode)
    {
        foreach (var dbClub in clubs)
        {
            var oldLokalen = await _db.ClubLokalen.Where(x => x.ClubId == dbClub.Id).ToArrayAsync();

            var frenoyClubs = await _frenoy.GetClubsAsync(new GetClubsRequest
            {
                GetClubs = new GetClubs()
                {
                    Club = getClubCode(dbClub)
                }
            });

            var frenoyClub = frenoyClubs.GetClubsResponse.ClubEntries.FirstOrDefault();
            if (frenoyClub == null)
            {
                Debug.Print("Got some wrong CodeSporta/Vttl in legacy db: " + dbClub.Name);
            }
            else if (frenoyClub.VenueEntries == null)
            {
                Debug.Print("Missing frenoy data?: " + dbClub.Name);
            }
            else if (frenoyClub.VenueEntries.Length < dbClub.Locations.Count)
            {
                Debug.Print("we got better data...: " + dbClub.Name);
            }
            else
            {
                _db.ClubLokalen.RemoveRange(oldLokalen);

                foreach (var frenoyLokaal in frenoyClub.VenueEntries)
                {
                    //Debug.Assert(string.IsNullOrWhiteSpace(frenoyLokaal.Comment), "comments opslaan in db?");
                    Debug.Assert(frenoyLokaal.ClubVenue == "1");
                    var lokaal = new ClubLocationEntity
                    {
                        Description = frenoyLokaal.Name,
                        Address = frenoyLokaal.Street,
                        ClubId = dbClub.Id,
                        City = frenoyLokaal.Town.Substring(frenoyLokaal.Town.IndexOf(" ") + 1),
                        Mobile = frenoyLokaal.Phone,
                        PostalCode = int.Parse(frenoyLokaal.Town.Substring(0, frenoyLokaal.Town.IndexOf(" "))),
                        MainLocation = frenoyLokaal.ClubVenue == "1"
                    };
                    _db.ClubLokalen.Add(lokaal);
                }
            }
        }
        await _db.SaveChangesAsync();
    }
    #endregion
}
