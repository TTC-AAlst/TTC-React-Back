﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text.RegularExpressions;
using Frenoy.Api;
using Frenoy.Api.FrenoyVttl;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;
using Match = Ttc.Model.Matches.Match;

namespace Frenoy.Api
{
    public class FrenoyMatchesApi : FrenoyApiBase
    {
        #region Constructor
        public FrenoyMatchesApi(ITtcDbContext ttcDbContext, Competition comp)
            : base(ttcDbContext, comp)
        {
            
        }
        #endregion

        #region Public API
        public void SyncTeamsAndMatches()
        {
            var frenoyTeams = _frenoy.GetClubTeams(new GetClubTeamsRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason
            });
            SyncTeamsAndMatches(frenoyTeams);
        }

        private void SyncTeamsAndMatches(GetClubTeamsResponse frenoyTeams)
        {
            foreach (var frenoyTeam in frenoyTeams.TeamEntries)
            {
                // Create new division for each team in the club
                // Check if it already exists: Two teams could play in the same division
                TeamEntity teamEntity = _db.Teams.SingleOrDefault(x => x.FrenoyDivisionId.ToString() == frenoyTeam.DivisionId && x.TeamCode == frenoyTeam.Team);
                if (teamEntity == null)
                {
                    teamEntity = CreateReeks(frenoyTeam);
                    _db.Teams.Add(teamEntity);
                    CommitChanges();

                    // Create the teams in the new division=reeks
                    var frenoyDivision = _frenoy.GetDivisionRanking(new GetDivisionRankingRequest
                    {
                        DivisionId = frenoyTeam.DivisionId
                    });
                    foreach (var frenoyTeamsInDivision in frenoyDivision.RankingEntries.Where(x => ExtractTeamCodeFromFrenoyName(x.Team) != frenoyTeam.Team || !IsOwnClub(x.TeamClub)))
                    {
                        var clubPloeg = CreateClubPloeg(teamEntity, frenoyTeamsInDivision);
                        _db.TeamOpponents.Add(clubPloeg);
                    }
                    CommitChanges();
                }

                // Create the matches=kalender table in the new  division=reeks
                GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    Club = _settings.FrenoyClub,
                    Season = _settings.FrenoySeason,
                    DivisionId = teamEntity.FrenoyDivisionId.ToString(),
                    Team = teamEntity.TeamCode,
                    WithDetailsSpecified = true,
                    WithDetails = true,
                });
                SyncMatches(teamEntity.Id, matches);
            }
        }

        private bool IsOwnClub(string teamClub)
        {
            return _settings.FrenoyClub == teamClub;
        }

        public void SyncMatch(int teamId, string frenoyMatchId)
        {
            GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason,
                WithDetailsSpecified = true,
                WithDetails = true,
                MatchId = frenoyMatchId
            });
            SyncMatches(teamId, matches);
        }

        public void SyncMatch(int teamId, string ploegCode, int weekName)
        {
            GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason,
                Team = ploegCode,
                WithDetailsSpecified = true,
                WithDetails = true,
                WeekName = weekName.ToString()
            });
            SyncMatches(teamId, matches);
        }

        public void SyncMatches(TeamEntity team, OpposingTeam opponent)
        {
            GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
            {
                Club = GetFrenoyClubdId(opponent.ClubId),
                Season = _settings.FrenoySeason,
                Team = opponent.TeamCode,
                WithDetailsSpecified = true,
                WithDetails = true,
                DivisionId = team.FrenoyDivisionId.ToString()
            });
            SyncMatches(team.Id, matches);
        }

        public void SyncMatches(int reeksId, GetMatchesResponse matches)
        {
            foreach (TeamMatchEntryType frenoyMatch in matches.TeamMatchesEntries.Where(x => x.HomeTeam.Trim() != "Vrij" && x.AwayTeam.Trim() != "Vrij"))
            {
                Debug.Assert(frenoyMatch.DateSpecified);
                Debug.Assert(frenoyMatch.TimeSpecified);

                // Kalender entries
                MatchEntity kalender = _db.Matches.SingleOrDefault(x => x.FrenoyMatchId == frenoyMatch.MatchId);
                if (kalender == null)
                {
                    kalender = CreateKalenderMatch(reeksId, frenoyMatch);
                    _db.Matches.Add(kalender);
                }

                // Wedstrijdverslagen
                if (!kalender.IsSyncedWithFrenoy && frenoyMatch.MatchDetails != null && frenoyMatch.MatchDetails.DetailsCreated)
                {
                    bool isForfeit = frenoyMatch.Score == null || frenoyMatch.Score.ToLowerInvariant().Contains("ff") || frenoyMatch.Score.ToLowerInvariant().Contains("af");
                    if (!isForfeit)
                    {
                        // Uitslag
                        kalender.HomeScore = int.Parse(frenoyMatch.Score.Substring(0, frenoyMatch.Score.IndexOf("-")));
                        kalender.AwayScore = int.Parse(frenoyMatch.Score.Substring(frenoyMatch.Score.IndexOf("-") + 1));
                        kalender.WalkOver = false;

                        // Spelers
                        var oldVerslagSpelers = _db.MatchPlayers.Where(x => x.MatchId == kalender.Id).ToArray();
                        _db.MatchPlayers.RemoveRange(oldVerslagSpelers);

                        AddVerslagPlayers(frenoyMatch.MatchDetails.HomePlayers.Players, kalender, true);
                        AddVerslagPlayers(frenoyMatch.MatchDetails.AwayPlayers.Players, kalender, false);

                        var testPlayer = kalender.Players.Count(x => x.PlayerId != 0);
                        if (testPlayer == 0 && (kalender.AwayTeamId.HasValue || kalender.HomeTeamId.HasValue))
                        {
                            var x = 5;
                        }

                        // Matchen
                        var oldVerslagenIndividueel = _db.MatchGames.Where(x => x.MatchId == kalender.Id).ToArray();
                        _db.MatchGames.RemoveRange(oldVerslagenIndividueel);

                        int id = 0;
                        foreach (var frenoyIndividual in frenoyMatch.MatchDetails.IndividualMatchResults)
                        {
                            MatchGameEntity matchResult;
                            int homeUniqueIndex, awayUniqueIndex;
                            if (!int.TryParse(frenoyIndividual.HomePlayerUniqueIndex, out homeUniqueIndex)
                                || !int.TryParse(frenoyIndividual.AwayPlayerUniqueIndex, out awayUniqueIndex))
                            {
                                // Sporta doubles match:
                                matchResult = new MatchGameEntity
                                {
                                    Id = id--,
                                    MatchId = kalender.Id,
                                    MatchNumber = int.Parse(frenoyIndividual.Position),
                                    WalkOver = WalkOver.None
                                };
                            }
                            else
                            {
                                // Sporta/Vttl singles match
                                matchResult = new MatchGameEntity
                                {
                                    Id = id--,
                                    MatchId = kalender.Id,
                                    MatchNumber = int.Parse(frenoyIndividual.Position),
                                    HomePlayerUniqueIndex = homeUniqueIndex,
                                    AwayPlayerUniqueIndex = awayUniqueIndex,
                                    WalkOver = WalkOver.None
                                };
                            }
                                
                            if (frenoyIndividual.IsHomeForfeited || frenoyIndividual.IsAwayForfeited)
                            {
                                matchResult.WalkOver = frenoyIndividual.IsHomeForfeited ? WalkOver.Home : WalkOver.Out;
                            }
                            else
                            {
                                matchResult.HomePlayerSets = int.Parse(frenoyIndividual.HomeSetCount);
                                matchResult.AwayPlayerSets = int.Parse(frenoyIndividual.AwaySetCount);
                            }
                            _db.MatchGames.Add(matchResult);
                        }
                    }
                    else
                    {
                        kalender.WalkOver = true;
                    }

                    kalender.IsSyncedWithFrenoy = true;
                }
                CommitChanges();
            }
        }

        private void AddVerslagPlayers(TeamMatchPlayerEntryType[] players, MatchEntity verslag, bool thuisSpeler)
        {
            foreach (var frenoyVerslagSpeler in players)
            {
                MatchPlayerEntity matchPlayerEntity = new MatchPlayerEntity
                {
                    MatchId = verslag.Id,
                    Ranking = frenoyVerslagSpeler.Ranking,
                    Home = thuisSpeler,
                    Name = GetSpelerNaam(frenoyVerslagSpeler),
                    Position = int.Parse(frenoyVerslagSpeler.Position),
                    UniqueIndex = int.Parse(frenoyVerslagSpeler.UniqueIndex)
                };
                if (frenoyVerslagSpeler.VictoryCount != null)
                {
                    matchPlayerEntity.Won = int.Parse(frenoyVerslagSpeler.VictoryCount);
                }
                else
                {
                    Debug.Assert(frenoyVerslagSpeler.IsForfeited, "Either a VictoryCount or IsForfeited");
                }

                PlayerEntity dbPlayer = null;
                if (verslag.IsHomeMatch.HasValue && ((verslag.IsHomeMatch.Value && thuisSpeler) || (!verslag.IsHomeMatch.Value && !thuisSpeler)))
                {
                    if (_isVttl)
                    {
                        dbPlayer = _db.Players.SingleOrDefault(x => x.ComputerNummerVttl.HasValue && x.ComputerNummerVttl.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
                    }
                    else
                    {
                        dbPlayer = _db.Players.SingleOrDefault(x => x.LidNummerSporta.HasValue && x.LidNummerSporta.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
                    }
                }
                if (dbPlayer != null)
                {
                    matchPlayerEntity.PlayerId = dbPlayer.Id;
                    if (!string.IsNullOrWhiteSpace(dbPlayer.NaamKort))
                    {
                        matchPlayerEntity.Name = dbPlayer.NaamKort;
                    }
                }

                _db.MatchPlayers.Add(matchPlayerEntity);
                verslag.Players.Add(matchPlayerEntity);
            }
        }

        private static string GetSpelerNaam(TeamMatchPlayerEntryType frenoyVerslagSpeler)
        {
            System.Globalization.TextInfo ti = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase((frenoyVerslagSpeler.FirstName + " " + frenoyVerslagSpeler.LastName).ToLowerInvariant());
        }
        #endregion

        #region Private Implementation
        private int GetSpelerId(string playerName)
        {
            var speler = _db.Players.Single(x => x.NaamKort == playerName);
            return speler.Id;
        }

        private readonly static Regex VttlReeksRegex = new Regex(@"Afdeling (\d+)(\w+)");
        private readonly static Regex SportaReeksRegex = new Regex(@"(\d)(\w)?");
        private TeamEntity CreateReeks(TeamEntryType frenoyTeam)
        {
            var reeks = new TeamEntity();
            reeks.Competition = _settings.Competitie;
            reeks.ReeksType = _settings.ReeksType;
            reeks.Year = _settings.Jaar;
            reeks.LinkId = $"{frenoyTeam.DivisionId}_{frenoyTeam.Team}";

            if (_isVttl)
            {
                var reeksMatch = VttlReeksRegex.Match(frenoyTeam.DivisionName);
                reeks.ReeksNummer = reeksMatch.Groups[1].Value;
                reeks.ReeksCode = reeksMatch.Groups[2].Value;
            }
            else
            {
                var reeksMatch = SportaReeksRegex.Match(frenoyTeam.DivisionName.Trim());
                reeks.ReeksNummer = reeksMatch.Groups[1].Value;
                reeks.ReeksCode = reeksMatch.Groups[2].Value;
            }

            reeks.FrenoyDivisionId = int.Parse(frenoyTeam.DivisionId);
            reeks.FrenoyTeamId = frenoyTeam.TeamId;
            reeks.TeamCode = frenoyTeam.Team;
            return reeks;
        }

        private MatchEntity CreateKalenderMatch(int reeksId, TeamMatchEntryType frenoyMatch)
        {
            var kalender = new MatchEntity
            {
                FrenoyMatchId = frenoyMatch.MatchId,
                Date = frenoyMatch.Date + new TimeSpan(frenoyMatch.Time.Hour, frenoyMatch.Time.Minute, 0),
                HomeClubId = GetClubId(frenoyMatch.HomeClub),
                HomeTeamCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.HomeTeam),
                AwayClubId = GetClubId(frenoyMatch.AwayClub),
                AwayPloegCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.AwayTeam),
                Week = int.Parse(frenoyMatch.WeekName),
            };

            //int weekName;
            //if (int.TryParse(frenoyMatch.WeekName, out weekName))
            //{
            //    kalender.Week = weekName;
            //}

            //TODO: we zaten hier for the derby problem
            // delete match id 563
            // do not pass reeksId here but find out what the Team is based on HomeClubId and HomeTeamCode

            if (kalender.HomeClubId == Constants.OwnClubId)
            {
                kalender.HomeTeamId = reeksId;
            }
            else if (kalender.AwayClubId == Constants.OwnClubId)
            {
                kalender.AwayTeamId = reeksId;
            }
            return kalender;
        }

        private TeamOpponentEntity CreateClubPloeg(TeamEntity teamEntity, RankingEntryType frenoyTeam)
        {
            var clubPloeg = new TeamOpponentEntity();
            clubPloeg.TeamId = teamEntity.Id;
            clubPloeg.ClubId = GetClubId(frenoyTeam.TeamClub);
            clubPloeg.TeamCode = ExtractTeamCodeFromFrenoyName(frenoyTeam.Team);
            return clubPloeg;
        }

        private string GetFrenoyClubdId(int clubId)
        {
            if (_isVttl)
            {
                return _db.Clubs.Single(x => x.Id == clubId).CodeVttl;
            }
            else
            {
                return _db.Clubs.Single(x => x.Id == clubId).CodeSporta;
            }
        }
        #endregion

        #region Debug & Tech Stuff
        [Conditional("DEBUG")]
        private void CheckPlayers()
        {
            foreach (string player in _settings.Players.Values.SelectMany(x => x))
            {
                try
                {
                    GetSpelerId(player);
                }
                catch (Exception ex)
                {
                    throw new Exception("No player with NaamKort " + player, ex);
                }
            }
        }
        #endregion
    }
}