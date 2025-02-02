using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Ttc.DataEntities;

[Table("Team")]
[Index(nameof(Year))]
public class TeamEntity : IAudit
{
    [Key]
    public int Id { get; set; }
    [MaxLength(10)]
    public Competition Competition { get; set; }
    /// <summary>
    /// ColumnName "Reeks" in the db
    /// </summary>
    [MaxLength(2)]
    public string DivisionNumber { get; set; } = "";
    [MaxLength(10)]
    public string DivisionType { get; set; } = "";
    [MaxLength(2)]
    public string DivisionCode { get; set; } = "";
    public int Year { get; set; }
    [MaxLength(10)]
    public string LinkId { get; set; } = "";
    [MaxLength(10)]
    public string FrenoyTeamId { get; set; } = "";
    public int FrenoyDivisionId { get; set; }

    public ICollection<TeamPlayerEntity> Players { get; set; } = [];
    public ICollection<TeamOpponentEntity> Opponents { get; set; } = [];

    /// <summary>
    /// TTC Aalst TeamCode (A, B, C, ...)
    /// </summary>
    [MaxLength(2)]
    public string TeamCode { get; set; } = "";

    public Audit Audit { get; } = new();

    public override string ToString() => $"Id={Id}, Competition={Competition} {Year}, Reeks={DivisionNumber}{DivisionCode}, FrenoyLink={LinkId}";
}
