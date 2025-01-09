using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[PrimaryKey(nameof(ClubId), nameof(PlayerId))]
[Table("ClubManager")]
public class ClubManagerEntity
{
    public int ClubId { get; set; }
    public int PlayerId { get; set; }

    public ClubEntity Club { get; set; }
    [StringLength(100)]
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }

    public override string ToString() => $"Club={ClubId}, Desc={Description}, Sort={SortOrder}, Player={PlayerId}";
}
