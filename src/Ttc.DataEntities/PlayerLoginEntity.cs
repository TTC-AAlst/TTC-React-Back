using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Table("PlayerLogin")]
public class PlayerLoginEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime At { get; set; }

    [StringLength(100)]
    public string Alias { get; set; } = "";
    public int PlayerId { get; set; }

    public override string ToString() => $"Alias={Alias} @ {At:dd/MM/yyyy}";
}
