using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ttc.DataEntities.Core;

namespace Ttc.DataEntities;

[Table("Parameter")]
public class ParameterEntity : IAudit
{
    [Key]
    [StringLength(20)]
    public string Key { get; set; } = "";
    [StringLength(2000)]
    public string Value { get; set; } = "";
    [StringLength(255)]
    public string? Description { get; set; }
    public Audit Audit { get; } = new();

    public override string ToString() => $"{Key}={Value}, Desc={Description}";
}
