﻿using System;
using Ttc.Model.Teams;

namespace Ttc.Model.Matches
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FrenoyMatchId { get; set; }

        public bool IsHomeMatch { get; set; }
        public int Week { get; set; }

        public int TeamId { get; set; }
        public OpposingTeam Opponent { get; set; }

        public MatchReport Report { get; set; }

        public override string ToString() => $"Id={Id} on {Date.ToString("g")}, Home={IsHomeMatch}, TeamId={TeamId}, Opponent=({Opponent})";
    }
}