namespace Ttc.WebApi.Emailing;

public class WeekCompetitionEmailModel
{
    /// <summary>
    /// If true send the email to just the current user
    /// </summary>
    public bool JustMe { get; set; }
    public string Title { get; set; } = "";
    public string Email { get; set; } = "";
    
    public override string ToString() => $"{Title}: {Email}";
}