﻿using Ttc.Model.Players;

namespace Ttc.Model.Teams;

/// <summary>
/// One TTC Aalst Team in a Division/Reeks
/// </summary>
public class Team
{
    #region Properties
    public int Id { get; set; }

    /// <summary>
    /// TTC Aalst Team code (A, B, C, ...)
    /// </summary>
    public string TeamCode { get; set; } = "";
    public int ClubId { get; set; }
    public ICollection<TeamPlayer> Players { get; set; } = [];

    public ICollection<OpposingTeam> Opponents { get; set; } = [];

    /// <summary>
    /// Vttl or Sporta
    /// </summary>
    public Competition Competition { get; set; }
    public int Year { get; set; }

    /// <summary>
    /// Vttl: 2A
    /// Sporta: 1
    /// Sporta Ere: empty
    /// </summary>
    public string DivisionName { get; set; } = "";

    /// <summary>
    /// Links to Frenoy website and API details of TTC Aalst Team
    /// </summary>
    public FrenoyTeamLinks Frenoy { get; set; }
    #endregion

    public override string ToString() => $"{Competition} {Year} {TeamCode}: {DivisionName}";
}
