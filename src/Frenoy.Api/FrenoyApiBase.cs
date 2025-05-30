﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using FrenoyVttl;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Frenoy.Api;

public class FrenoyApiBase
{
    #region Fields
    const string FrenoyVttlWsdlUrl = "https://api.vttl.be/?wsdl";
    const string FrenoySportaWsdlUrl = "https://ttonline.sporta.be/api/?wsdl";
    const string FrenoyVttlEndpoint = "https://api.vttl.be/index.php?s=vttl";
    const string FrenoySportaEndpoint = "https://ttonline.sporta.be/api/index.php?s=sporcrea";

    protected readonly FrenoySettings _settings;
    protected readonly TabTAPI_PortTypeClient _frenoy;
    protected readonly bool _isVttl;
    protected readonly ITtcDbContext _db;
    protected readonly int _thuisClubId;
    protected readonly bool _forceSync;
    protected readonly int _currentSeason;
    #endregion

    #region Constructor
    public FrenoyApiBase(ITtcDbContext ttcDbContext, Competition comp, bool forceSync = false)
    {
        _forceSync = forceSync;
        _db = ttcDbContext;

        bool isVttl = comp is Competition.Vttl or Competition.Jeugd;
        _currentSeason = _db.CurrentSeason;
        _settings = isVttl ? FrenoySettings.VttlSettings(_currentSeason) : FrenoySettings.SportaSettings(_currentSeason);

        _isVttl = isVttl;

        var binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport)
        {
            MaxReceivedMessageSize = 20000000,
            MaxBufferSize = 20000000,
            MaxBufferPoolSize = 20000000,
            AllowCookies = true
        };

        if (isVttl)
        {
            _frenoy = new FrenoyVttl.TabTAPI_PortTypeClient(
                binding,
                new System.ServiceModel.EndpointAddress(new Uri(FrenoyVttlEndpoint))
            );
            _thuisClubId = _db.Clubs.Single(x => x.CodeVttl == _settings.FrenoyClub).Id;
        }
        else
        {
            // Sporta
            _thuisClubId = _db.Clubs.Single(x => x.CodeSporta == _settings.FrenoyClub).Id;

            //var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            // binding.Security.Mode = BasicHttpSecurityMode.Transport;
            //var endpoint = new EndpointAddress(FrenoySportaEndpoint);
            _frenoy = new TabTAPI_PortTypeClient(
                binding,
                new System.ServiceModel.EndpointAddress(new Uri(FrenoySportaEndpoint))
            );
        }

        // Turn off certificate check -- probably a problem with a self signed certificate on the Frenoy server?
        // SecurityNegotiationException: 'Could not establish secure channel for SSL/TLS with authority 'api.vttl.be'.
        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
        //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        //ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, error) => true;

        //_frenoy.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(5);
        //_frenoy.Endpoint.Binding.CloseTimeout = TimeSpan.FromMinutes(5);
        //_frenoy.Endpoint.Binding.OpenTimeout = TimeSpan.FromMinutes(5);
        //_frenoy.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);
    }
    #endregion

    private static readonly Regex ClubHasTeamCodeRegex = new(@"(\w)( \(af\))?$");
    protected static string? ExtractTeamCodeFromFrenoyName(string team)
    {
        // team == Sint-Niklase Tafeltennisclub D
        var regMatch = ClubHasTeamCodeRegex.Match(team);
        if (regMatch.Success)
        {
            return regMatch.Groups[1].Value;
        }
        return null;
    }

    protected static bool ExtractIsForfaitFromFrenoyName(string team)
    {
        var regMatch = ClubHasTeamCodeRegex.Match(team);
        return regMatch.Groups[2].Success;
    }

    protected async Task<int> GetClubId(string frenoyClubCode)
    {
        ClubEntity? club;
        if (_isVttl)
        {
            club = await _db.Clubs.SingleOrDefaultAsync(x => x.CodeVttl == frenoyClubCode);
        }
        else
        {
            club = await _db.Clubs.SingleOrDefaultAsync(x => x.CodeSporta == frenoyClubCode);
        }
        if (club == null)
        {
            club = await CreateClub(frenoyClubCode);
        }
        return club.Id;
    }

    private async Task<ClubEntity> CreateClub(string frenoyClubCode)
    {
        var frenoyClub = await _frenoy.GetClubsAsync(new GetClubsRequest
        {
            GetClubs = new GetClubs()
            {
                Club = frenoyClubCode,
                Season = _settings.FrenoySeason.ToString()
            }
        });
        Debug.Assert(frenoyClub.GetClubsResponse.ClubEntries.Count() == 1);

        var club = new ClubEntity
        {
            CodeVttl = _isVttl ? frenoyClubCode : null,
            CodeSporta = !_isVttl ? frenoyClubCode : null,
            Active = true,
            Name = frenoyClub.GetClubsResponse.ClubEntries.First().LongName,
            Shower = false
        };

        _db.Clubs.Add(club);
        await CommitChanges();

        foreach (var frenoyLocation in frenoyClub.GetClubsResponse.ClubEntries.First().VenueEntries)
        {
            var location = new ClubLocationEntity
            {
                ClubId = club.Id,
                Mobile = frenoyLocation.Phone,
                Description = frenoyLocation.Name,
                Address = frenoyLocation.Street,
                PostalCode = int.Parse(frenoyLocation.Town.Substring(0, frenoyLocation.Town.IndexOf(" "))),
                City = frenoyLocation.Town.Substring(frenoyLocation.Town.IndexOf(" ") + 1),
                MainLocation = true
            };
            _db.ClubLocations.Add(location);
        }

        return club;
    }

    protected async Task CommitChanges()
    {
        await _db.SaveChangesAsync();
    }
}
