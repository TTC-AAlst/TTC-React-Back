using Ttc.Model.Players;

namespace Ttc.Model.Matches;

public class PreviousEncounter
{
    public int RequestMatchId { get; set; }
    public int MatchId { get; set; }
    public DateTime MatchDate { get; set; }
    public Competition Competition { get; set; }

    public string HomeName { get; set; } = "";
    public int HomePlayerUniqueId { get; set; }
    public int HomePlayerSets { get; set; }
    public string HomeRanking { get; set; } = "";

    public string AwayName { get; set; } = "";
    public int AwayPlayerUniqueId { get; set; }
    public int AwayPlayerSets { get; set; }
    public string AwayRanking { get; set; } = "";

    public override string ToString() => $"{HomePlayerUniqueId} vs {AwayPlayerUniqueId} ({HomePlayerSets}-{AwayPlayerSets})";
}

public class PreviousEncounterRequest
{
    public int MatchId { get; set; }
    public Competition Competition { get; set; }

    /// <summary>
    /// Keys=Name, Values=UniqueId
    /// </summary>
    public Dictionary<string, int> OpponentPlayerNames { get; set; } = [];
    /// <summary>
    /// Keys=PlayerId, Values=UniqueId
    /// </summary>
    public Dictionary<int, int> OwnPlayerIds { get; set; } = [];

    public override string ToString() => $"Match={MatchId} ({Competition})";
}