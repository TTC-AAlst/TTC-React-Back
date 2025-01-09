using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Flags]
public enum PlayerToegang
{
    Player = 1,
    /// <summary>
    /// Dirk DS, ...
    /// </summary>
    Board = 7,
    /// <summary>
    /// Jorn en ik
    /// </summary>
    Dev = 8,
    /// <summary>
    /// Algemeen toegankelijke computer in het clublokaal
    /// </summary>
    System = 9,
}

[Table(TableName)]
public class PlayerEntity
{
    public const string TableName = "Player";

    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }
    [StringLength(100)]
    public string? LastName { get; set; }
    /// <summary>
    /// FirstName + LastName
    /// </summary>
    [NotMapped]
    public string Name => $"{FirstName} {LastName}";

    [StringLength(20)]
    public string? Alias { get; set; }
    public PlayerToegang Security { get; set; }
    [StringLength(50)]
    public string? Style { get; set; }
    [StringLength(200)]
    public string? BestStroke { get; set; }

    /// <summary>
    /// Has a key to enter the physical club
    /// </summary>
    public bool? HasKey { get; set; }

    #region Vttl
    public int? ClubIdVttl { get; set; }
    public int? IndexVttl { get; set; }
    [StringLength(250)]
    public string? FrenoyLinkVttl { get; set; }
    [StringLength(5)]
    public string? RankingVttl { get; set; }
    [StringLength(5)]
    public string? NextRankingVttl { get; set; }
    public int? ComputerNummerVttl { get; set; }
    public int? VolgnummerVttl { get; set; }
    #endregion

    #region Sporta
    public int? ClubIdSporta { get; set; }
    public int? IndexSporta { get; set; }
    [StringLength(250)]
    public string? FrenoyLinkSporta { get; set; }
    [StringLength(5)]
    public string? RankingSporta { get; set; }
    [StringLength(5)]
    public string? NextRankingSporta { get; set; }
    public int? LidNummerSporta { get; set; }
    public int? VolgnummerSporta { get; set; }
    #endregion

    #region Address
    [StringLength(250)]
    public string? Address { get; set; }
    [StringLength(250)]
    public string? City { get; set; }
    [StringLength(20)]
    public string? Mobile { get; set; }
    [StringLength(250)]
    public string? Email { get; set; }
    #endregion

    /// <summary>
    /// Jaar gestopt. <see cref="Active"/>?
    /// </summary>
    public int? QuitYear { get; set; }
    public bool Active => QuitYear == null;

    public bool IsFromOwnClub() => ClubIdSporta == Constants.OwnClubId || ClubIdVttl == Constants.OwnClubId;

    public override string ToString() => $"Id={Id}, Alias={Alias} ({ClubIdVttl}, {ClubIdSporta}), Vttl={RankingVttl}, Sporta={RankingSporta}, Active={Active}";
}
