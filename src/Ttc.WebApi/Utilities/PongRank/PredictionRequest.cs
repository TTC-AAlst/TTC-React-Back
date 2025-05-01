using Ttc.Model.Players;

namespace Ttc.WebApi.Utilities.PongRank;

/// <summary>
/// Predict next ranking for entire club
/// </summary>
public class PredictionRequest
{
    public Competition Competition { get; set; }
    public int Year { get; set; }
    public string ClubUniqueIndex { get; set; } = "";

    public override string ToString() => $"{Competition} {Year}, Club={ClubUniqueIndex}";
}