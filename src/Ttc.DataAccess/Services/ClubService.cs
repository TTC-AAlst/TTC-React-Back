using AutoMapper;
using Frenoy.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Clubs;
using Ttc.Model.Core;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services;

public class ClubService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;
    private readonly TtcLogger _logger;
    private readonly CacheHelper _cache;

    public ClubService(ITtcDbContext context, IMapper mapper, IMemoryCache cache, TtcLogger logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = new CacheHelper(cache);
    }

    public async Task<CacheResponse<Club>?> GetActiveClubs(DateTime? lastChecked)
    {
        var clubs = await _cache.GetOrSet("clubs", GetActiveClubs, TimeSpan.FromHours(5));
        //if (lastChecked.HasValue && lastChecked.Value >= clubs.LastChange)
        //{
        //    return null;
        //}
        return clubs;
    }

    private async Task<CacheResponse<Club>> GetActiveClubs()
    {
        var activeClubs = await _context.Clubs
            .Include(x => x.Locations)
            .Where(x => x.Active)
            .ToListAsync();

        var lastChange = activeClubs.Max(x => x.Audit.ModifiedOn) ?? DateTime.MinValue;
        var result = _mapper.Map<IList<ClubEntity>, IList<Club>>(activeClubs);


        var ourClub = result.Single(x => x.Id == Constants.OwnClubId);
        var managers = await _context.ClubManagers.ToArrayAsync();
        ourClub.Managers = managers
            .Where(x => x.ClubId == Constants.OwnClubId)
            .Select(x => new ClubManager()
            {
                PlayerId = x.PlayerId,
                Description = x.Description,
                SortOrder = x.SortOrder,
            })
            .ToArray();

        return new CacheResponse<Club>(result, lastChange);
    }

    #region Club Board
    public async Task SaveBoardMember(int playerId, string boardFunction, int sort)
    {
        var board = await _context.ClubManagers.SingleOrDefaultAsync(x => x.PlayerId == playerId);
        if (board == null)
        {
            board = new ClubManagerEntity()
            {
                ClubId = Constants.OwnClubId,
                PlayerId = playerId
            };
            await _context.ClubManagers.AddAsync(board);
        }

        board.Description = boardFunction;
        board.SortOrder = sort;
        await ChangeClub(board.ClubId);
        await _context.SaveChangesAsync();
        _cache.Remove("clubs");
    }

    public async Task DeleteBoardMember(int playerId)
    {
        var board = await _context.ClubManagers.SingleAsync(x => x.PlayerId == playerId);
        _context.ClubManagers.Remove(board);
        await ChangeClub(board.ClubId);
        await _context.SaveChangesAsync();
        _cache.Remove("clubs");
    }
    #endregion

    public async Task<Club> UpdateClub(Club club)
    {
        var existingClub = await _context.Clubs
            .Include(x => x.Locations)
            .FirstOrDefaultAsync(x => x.Id == club.Id);
        if (existingClub == null)
        {
            throw new Exception("Club not found");
        }

        MapClub(club, existingClub);
        await ChangeClub(club.Id);
        await _context.SaveChangesAsync();
        _cache.Remove("clubs");
        return club;
    }

    public async Task Sync()
    {
        var sportaApi = new FrenoyClubApi(_context, _logger, Competition.Sporta);
        await sportaApi.SyncClubVenues();

        var vttlApi = new FrenoyClubApi(_context, _logger, Competition.Vttl);
        await vttlApi.SyncClubVenues();

        _cache.Remove("clubs");
    }

    private async Task ChangeClub(int clubId)
    {
        var club = await _context.Clubs.FindAsync(clubId);
        if (club != null)
        {
            club.Audit.ModifiedOn = DateTime.Now;
        }
    }

    private static void MapClub(Club club, ClubEntity existingClub)
    {
        existingClub.Name = club.Name;
        existingClub.Shower = club.Shower;
        existingClub.Website = club.Website;

        // Map MainLocation
        if (club.MainLocation != null)
        {
            var mainLocationEntity = existingClub.Locations.FirstOrDefault(x => x.MainLocation);
            if (mainLocationEntity != null)
            {
                MapLocation(club.MainLocation, mainLocationEntity);
            }
            else
            {
                mainLocationEntity = new ClubLocationEntity { MainLocation = true, ClubId = existingClub.Id };
                MapLocation(club.MainLocation, mainLocationEntity);
                existingClub.Locations.Add(mainLocationEntity);
            }
        }

        var incomingAlternativeIds = club.AlternativeLocations
            .Where(x => x.Id != 0)
            .Select(x => x.Id)
            .ToHashSet();

        // Delete locations that no longer exist
        var locationsToDelete = existingClub.Locations
            .Where(x => !x.MainLocation && !incomingAlternativeIds.Contains(x.Id))
            .ToList();
        foreach (var location in locationsToDelete)
        {
            existingClub.Locations.Remove(location);
        }

        // Update or add alternative locations
        foreach (var altLocation in club.AlternativeLocations)
        {
            if (altLocation.Id == 0)
            {
                // Add new location
                var newLocation = new ClubLocationEntity { MainLocation = false, ClubId = existingClub.Id };
                MapLocation(altLocation, newLocation);
                existingClub.Locations.Add(newLocation);
            }
            else
            {
                // Update existing location
                var existingLocation = existingClub.Locations.FirstOrDefault(x => x.Id == altLocation.Id);
                if (existingLocation != null)
                {
                    MapLocation(altLocation, existingLocation);
                }
            }
        }
    }

    private static void MapLocation(ClubLocation source, ClubLocationEntity target)
    {
        target.Description = source.Description;
        target.Address = source.Address;
        target.City = source.City;
        target.PostalCode = int.TryParse(source.PostalCode, out var postalCode) ? postalCode : 0;
        target.Mobile = source.Mobile;
        target.Comment = source.Comment;
    }
}
