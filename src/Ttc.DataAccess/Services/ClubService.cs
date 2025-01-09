using AutoMapper;
using Ttc.DataEntities;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities.Core;
using Ttc.Model.Clubs;

namespace Ttc.DataAccess.Services;

public class ClubService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;

    public ClubService(ITtcDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Club>> GetActiveClubs()
    {
        var activeClubs = await _context.Clubs
            .Include(x => x.Locations)
            .Include(x => x.Managers)
            .Where(x => x.Active)
            .ToListAsync();

        var result = _mapper.Map<IList<ClubEntity>, IList<Club>>(activeClubs);

        var managers = activeClubs.Single(x => x.Id == Constants.OwnClubId).Managers;
        var managerIds = managers.Select(x => x.PlayerId).ToArray();


        var ourClub = result.Single(x => x.Id == Constants.OwnClubId);
        ourClub.Managers = new List<ClubManager>();

        var managerPlayers = await _context.Players.Where(x => managerIds.Contains(x.Id)).ToArrayAsync();
        foreach (var managerPlayer in managerPlayers)
        {
            var managerInfo = managers.Single(x => x.PlayerId == managerPlayer.Id);
            ourClub.Managers.Add(new ClubManager
            {
                Description = managerInfo.Description,
                PlayerId = managerInfo.PlayerId,
                Name = managerPlayer.Name,
                SortOrder = managerInfo.SortOrder
            });
        }

        return result;
    }

    #region Club Board
    public async Task SaveBoardMember(int playerId, string boardFunction, int sort)
    {
        var board = await _context.ClubContacten.SingleOrDefaultAsync(x => x.PlayerId == playerId);
        if (board == null)
        {
            board = new ClubManagerEntity()
            {
                ClubId = Constants.OwnClubId,
                PlayerId = playerId
            };
            _context.ClubContacten.Add(board);
        }

        board.Description = boardFunction;
        board.SortOrder = sort;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBoardMember(int playerId)
    {
        var board = await _context.ClubContacten.SingleAsync(x => x.PlayerId == playerId);
        _context.ClubContacten.Remove(board);
        await _context.SaveChangesAsync();
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
        await _context.SaveChangesAsync();
        return club;
    }

    private static void MapClub(Club club, ClubEntity existingClub)
    {
        existingClub.Name = club.Name;
        existingClub.Shower = club.Shower;
        existingClub.Website = club.Website;
        // existingClub.Lokalen = club.MainLocation
    }
}
