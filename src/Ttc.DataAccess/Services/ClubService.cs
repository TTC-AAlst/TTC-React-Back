using AutoMapper;
using Ttc.DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities.Core;
using Ttc.Model.Clubs;

namespace Ttc.DataAccess.Services;

public class ClubService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;
    private readonly CacheHelper _cache;

    public ClubService(ITtcDbContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = new CacheHelper(cache);
    }

    public async Task<ClubCache?> GetActiveClubs(DateTime? lastChecked)
    {
        var clubs = await _cache.GetOrSet("clubs", GetActiveClubs, TimeSpan.FromHours(1));
        if (lastChecked.HasValue && lastChecked.Value >= clubs.LastChange)
        {
            return null;
        }
        return clubs;
    }

    private async Task<ClubCache> GetActiveClubs()
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

        return new ClubCache(result, lastChange);
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
        var existingClub = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == club.Id);
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
        // existingClub.Lokalen = club.MainLocation
    }
}
