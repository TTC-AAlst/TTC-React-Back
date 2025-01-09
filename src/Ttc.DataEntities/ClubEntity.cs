using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Clubs;

namespace Ttc.DataEntities;

/// <summary>
/// Entity suffix: Otherwise conflict with <see cref="Club"/>
/// </summary>
[Table("Club")]
public class ClubEntity
{
    [Key]
    public int Id { get; set; }
    [StringLength(50)]
    public string Name { get; set; } = "";
    [StringLength(10)]
    public string? CodeVttl { get; set; }
    [StringLength(10)]
    public string? CodeSporta { get; set; }
    public bool Active { get; set; }
    public bool Shower { get; set; }
    [StringLength(255)]
    public string? Website { get; set; }

    public ICollection<ClubLocationEntity> Locations { get; set; } = [];

    public ICollection<ClubManagerEntity> Managers { get; set; } = [];

    public override string ToString() => $"Id={Id}, Name={Name}, Vttl={CodeVttl}, Sporta={CodeSporta}, Active={Active}";
}
