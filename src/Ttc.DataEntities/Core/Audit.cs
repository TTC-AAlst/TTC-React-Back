using System.ComponentModel.DataAnnotations;

namespace Ttc.DataEntities.Core;

public class Audit
{
    public DateTime CreatedOn { get; set; }
    [StringLength(50)]
    public string CreatedBy { get; set; } = "";
    public DateTime? ModifiedOn { get; set; }
    [StringLength(50)]
    public string? ModifiedBy { get; set; }

    public override string ToString() => $"LastBy={ModifiedBy ?? CreatedBy}, On={ModifiedOn ?? ModifiedOn}";
}
