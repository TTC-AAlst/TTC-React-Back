using Ttc.Model.Players;

namespace Ttc.WebApi.Utilities.PongRank;

public class PredictionResult
{
    public Competition Competition { get; set; }
    public int UniqueIndex { get; set; }
    public string Name { get; set; } = "";
    public string OldRanking { get; set; } = "";
    public string NewRanking { get; set; } = "";

    public override string ToString() => $"{Competition} {Name}: {OldRanking} -> {NewRanking}";
}