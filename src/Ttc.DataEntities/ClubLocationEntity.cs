using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ttc.DataEntities;

[Table("ClubLocation")]
public class ClubLocationEntity
{
    [Key]
    public int Id { get; set; }
    [StringLength(250)]
    public string Description { get; set; } = "";
    [StringLength(250)]
    public string Address { get; set; } = "";
    [StringLength(250)]
    public string City { get; set; } = "";
    public bool MainLocation { get; set; }
    public int PostalCode { get; set; }
    [StringLength(50)]
    public string Mobile { get; set; } = "";

    [ForeignKey("ClubId")]
    public ClubEntity Club { get; set; }
    public int ClubId { get; set; }
    [StringLength(4000)]
    public string? Comment { get; set; }

    public override string ToString() => $"Id={Id}, ClubId={ClubId}, Desc={Description}, Address={Address}, City={City}";
}
