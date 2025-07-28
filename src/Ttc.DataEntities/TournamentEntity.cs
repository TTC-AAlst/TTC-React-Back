using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.Model.Players;

namespace Ttc.DataEntities;

[Table("Tournament")]
public class TournamentEntity
{
    [Key]
    public int Id { get; set; }
    public Competition Competition { get; set; }
    [StringLength(255)]
    public string Name { get; set; } = "";
    [StringLength(50)]
    public string UniqueIndex { get; set; } = "";
    public DateTime Date { get; set; }
    public int FrenoySeason { get; set; }

    public override string ToString() => $"{Name} on {Date:d}";
}
