using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Table("Parameter")]
public class ParameterEntity
{
    [Key]
    [StringLength(20)]
    public string Key { get; set; } = "";
    [StringLength(255)]
    public string Value { get; set; } = "";
    [StringLength(255)]
    public string? Description { get; set; }

    public override string ToString() => $"{Key}={Value}, Desc={Description}";
}
