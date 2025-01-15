namespace Ttc.Model.Clubs;

/// <summary>
/// Voorzitter, secretaris, ...
/// </summary>
public class ClubManager
{
    #region Properties
    public int PlayerId { get; set; }
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
    #endregion

    public override string ToString() => $"Player={PlayerId}, Desc={Description}";
}
