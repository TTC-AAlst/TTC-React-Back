using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Players;
using AutoMapper;
using Frenoy.Api;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities.Core;
using Microsoft.Extensions.Caching.Memory;
using Ttc.DataAccess.Utilities.Excel;
using Ttc.Model.Clubs;

namespace Ttc.DataAccess.Services;

public class PlayerService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;
    private readonly CacheHelper _cache;

    public PlayerService(ITtcDbContext context, IMapper mapper, IMemoryCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = new CacheHelper(cache);
    }

    #region Player

    public async Task<CacheResponse<Player>?> GetOwnClub(DateTime? lastChecked)
    {
        var players = await _cache.GetOrSet("players", GetOwnClub, TimeSpan.FromHours(1));
        if (lastChecked.HasValue && lastChecked.Value >= players.LastChange)
        {
            return null;
        }
        return players;
    }

    private async Task<CacheResponse<Player>> GetOwnClub()
    {
        var players = await _context.Players
            .Where(x => x.QuitYear == null)
            .ToArrayAsync();

        var lastChange = players.Max(x => x.Audit.ModifiedOn) ?? DateTime.MinValue;
        var result = _mapper.Map<IList<PlayerEntity>, IList<Player>>(players);
        return new CacheResponse<Player>(result, lastChange);
    }

    public async Task<IEnumerable<Player>> GetQuitters()
    {
        var players = await _context.Players
            .Where(x => x.QuitYear != null)
            .ToArrayAsync();

        var result = _mapper.Map<IList<PlayerEntity>, IList<Player>>(players);
        return result;
    }

    public async Task<Player> GetPlayer(int playerId)
    {
        var player = await _context.Players.SingleAsync(x => x.Id == playerId);
        var newPlayer = _mapper.Map<PlayerEntity, Player>(player);
        return newPlayer;
    }

    public async Task<Player?> UpdateStyle(PlayerStyle playerStyle)
    {
        var existingPlayer= await _context.Players.SingleOrDefaultAsync(x => x.Id == playerStyle.PlayerId);
        if (existingPlayer == null)
        {
            return null;
        }

        existingPlayer.Style = playerStyle.Name;
        existingPlayer.BestStroke = playerStyle.BestStroke;
        await _context.SaveChangesAsync();
        _cache.Remove("players");
        var updatedPlayer = await GetPlayer(playerStyle.PlayerId);
        return updatedPlayer;
    }

    public async Task<Player> UpdatePlayer(Player player)
    {
        var existingPlayer= await _context.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
        if (existingPlayer == null)
        {
            existingPlayer = new PlayerEntity();
            MapPlayer(player, existingPlayer);
            await _context.Players.AddAsync(existingPlayer);
        }
        else
        {
            MapPlayer(player, existingPlayer);
        }

        await _context.SaveChangesAsync();
        _cache.Remove("players");
        player.Id = existingPlayer.Id;
        var newPlayer = await GetPlayer(player.Id);
        return newPlayer;
    }

    public async Task DeletePlayer(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null) return;
        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        _cache.Remove("players");
    }

    private static void MapPlayer(Player player, PlayerEntity existingPlayer)
    {
        existingPlayer.Mobile = player.Contact.Mobile;
        existingPlayer.Email = player.Contact.Email;
        existingPlayer.Address = player.Contact.Address;
        existingPlayer.City = player.Contact.City;

        existingPlayer.Style = player.Style.Name;
        existingPlayer.BestStroke = player.Style.BestStroke;

        existingPlayer.QuitYear = player.QuitYear;
        existingPlayer.Security = (PlayerAccess)Enum.Parse(typeof(PlayerAccess), player.Security);
        existingPlayer.HasKey = player.HasKey;

        existingPlayer.FirstName = player.FirstName;
        existingPlayer.LastName = player.LastName;
        existingPlayer.Alias = player.Alias;

        existingPlayer.NextRankingVttl = player.Vttl?.NextRanking;
        existingPlayer.NextRankingSporta = player.Sporta?.NextRanking;
    }

    public async Task<byte[]> GetExcelExport()
    {
        var activePlayers = await _context.Players.Where(x => x.QuitYear == null).ToArrayAsync();
        var excelCreator = new PlayersExcelCreator(activePlayers);
        return excelCreator.Create();
    }
    #endregion

    #region User
    public async Task<User> GetUser(int playerId)
    {
        int currentSeason = _context.CurrentSeason;
        var teams = await _context.Teams
            .Include(x => x.Players)
            .Where(x => x.Year == currentSeason)
            .Where(x => x.Players.Any(ply => ply.PlayerId == playerId))
            .Select(x => x.Id)
            .ToListAsync();

        var player = await _context.Players.SingleAsync(ply => ply.Id == playerId && ply.QuitYear == null);
        return new User
        {
            PlayerId = playerId,
            Alias = player.Alias ?? "",
            Security = GetPlayerSecurity(player.Security),
            Teams = teams
        };
    }

    private static string[] GetPlayerSecurity(PlayerAccess access)
    {
        switch (access)
        {
            case PlayerAccess.System:
                return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN", "IS_SYSTEM" };

            case PlayerAccess.Dev:
                return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN", "IS_DEV" };

            case PlayerAccess.Board:
                return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN" };

            // No PlayerToegang.Captain: This happens automatically when assigned Captain to a Team

            case PlayerAccess.Player:
            default:
                return [];
        }
    }
    #endregion

    public async Task FrenoySync()
    {
        var vttlPlayers = new FrenoyPlayersApi(_context, Competition.Vttl);
        await vttlPlayers.StopAllPlayers(false);
        await vttlPlayers.SyncPlayers();
        var sportaPlayers = new FrenoyPlayersApi(_context, Competition.Sporta);
        await sportaPlayers.SyncPlayers();
    }

    #region Login & Password
    private const int SystemPlayerIdFromFrontend = -1;
    public async Task<User?> Login(UserCredentials user)
    {
        if (user.PlayerId == SystemPlayerIdFromFrontend)
        {
            user.PlayerId = (await _context.Players.SingleAsync(ply => ply.Alias == "SYSTEM")).Id;
        }

        var playerEntity = await _context.Players.FromSqlRaw(
            $"SELECT * FROM {PlayerEntity.TableName} WHERE id={{0}} AND Password=MD5({{1}}) AND QuitYear IS NULL",
            new MySqlParameter("@p1", user.PlayerId),
            new MySqlParameter("@p2", user.Password)
        ).FirstOrDefaultAsync();

        if (playerEntity == null)
        {
            return null;
        }

        return await GetUser(user.PlayerId);
    }

    public async Task<User?> ChangePassword(PasswordCredentials userNewCredentials)
    {
        var player = await _context.Players.FromSqlRaw(
            $"SELECT * FROM {PlayerEntity.TableName} WHERE id={{0}} AND Password=MD5({{1}})",
            userNewCredentials.PlayerId,
            userNewCredentials.OldPassword).FirstOrDefaultAsync();

        if (player == null)
        {
            return null;
        }

        await _context.Database.ExecuteSqlRawAsync(
            $"UPDATE {PlayerEntity.TableName} SET Password=MD5({{1}}) WHERE id={{0}}",
            userNewCredentials.PlayerId,
            userNewCredentials.NewPassword);

        return await GetUser(userNewCredentials.PlayerId);
    }

    public async Task<string?> SetNewPassword(PasswordCredentials request)
    {
        PlayerEntity? player;
        if (request.PlayerId == SystemPlayerIdFromFrontend)
        {
            player = await _context.Players.SingleAsync(ply => ply.Alias == "SYSTEM");
        }
        else
        {
            player = await _context.Players.SingleOrDefaultAsync(x => x.Id == request.PlayerId);
        }

        if (player != null)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"UPDATE {PlayerEntity.TableName} SET Password=MD5({{1}}) WHERE id={{0}}",
                player.Id,
                request.NewPassword);

            return player.Email;
        }
        return null;
    }

    public async Task<Guid> EmailMatchesPlayer(string email, int playerId)
    {
        var player = await _context.Players.SingleOrDefaultAsync(x => x.Id == playerId && x.Email.ToLower() == email.ToLower());
        if (player == null)
        {
            throw new Exception("Email komt niet overeen voor " + playerId);
        }

        var passwordReset = new PlayerPasswordResetEntity(playerId);
        _context.PlayerPasswordResets.Add(passwordReset);
        await _context.SaveChangesAsync();

        return passwordReset.Guid;
    }

    public async Task SetNewPasswordFromGuid(Guid guid, int playerId, string password)
    {
        var now = DateTime.UtcNow;
        var resetInfo = await _context.PlayerPasswordResets
            .Where(x => x.Guid == guid)
            .Where(x => x.PlayerId == playerId)
            .FirstOrDefaultAsync(x => x.ExpiresOn > now);

        if (resetInfo != null)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"UPDATE {PlayerEntity.TableName} SET Password=MD5({{1}}) WHERE id={{0}}",
                playerId,
                password);
        }
        else
        {
            throw new Exception($"Geen reset link gevonden {guid} voor speler {playerId}");
        }
    }
    #endregion
}
