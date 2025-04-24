using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Utilities.AutoMapperConfig;

internal class PlayerProfile : Profile
{
    public PlayerProfile()
    {
        CreateMap<Player, Player>();

        CreateMap<PlayerEntity, Player>()
            .ForMember(
                dest => dest.Alias,
                opts => opts.MapFrom(src => src.Alias))
            .ForMember(
                dest => dest.Active,
                opts => opts.MapFrom(src => src.Active))
            .ForMember(
                dest => dest.QuitYear,
                opts => opts.MapFrom(src => src.QuitYear))
            .ForMember(
                dest => dest.Security,
                opts => opts.MapFrom(src => src.Security.ToString()))
            .ForMember(
                dest => dest.Style,
                opts => opts.MapFrom(src => new PlayerStyle(src.Id, src.Style, src.BestStroke)))
            .ForMember(
                dest => dest.Contact,
                opts => opts.MapFrom(src => new PlayerContact(src.Id, src.Email, src.Mobile, src.Address, src.City)))
            .ForMember(
                dest => dest.Vttl,
                opts => opts.MapFrom(src => src.ClubIdVttl.HasValue ?
                    CreateVttlPlayer(src.ClubIdVttl.Value, src.ComputerNummerVttl.Value, src.FrenoyLinkVttl, src.RankingVttl, src.VolgnummerVttl.Value, src.IndexVttl.Value, src.NextRankingVttl)
                    : null))
            .ForMember(
                dest => dest.Sporta,
                opts => opts.MapFrom(src => src.ClubIdSporta.HasValue ?
                    CreateSportaPlayer(src.ClubIdSporta.Value, src.LidNummerSporta.Value, src.FrenoyLinkSporta, src.RankingSporta, src.VolgnummerSporta.Value, src.IndexSporta.Value, src.NextRankingSporta)
                    : null))
            ;

        CreateMap<EventEntity, EventModel>()
            .ForMember(
                dest => dest.CreatedOn,
                opts => opts.MapFrom(x => x.Audit.CreatedOn))
            .ForMember(
                dest => dest.CreatedBy,
                opts => opts.MapFrom(x => x.Audit.CreatedBy))
            ;
    }

    private static PlayerCompetition CreateSportaPlayer(int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex, string nextRanking)
    {
        return new PlayerCompetition(
            Competition.Sporta,
            clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, KlassementValueConverter.Sporta(ranking), nextRanking);
    }

    private static PlayerCompetition CreateVttlPlayer(int clubId, int uniqueIndex, string frenoyLink, string ranking, int position, int rankingIndex, string nextRanking)
    {
        return new PlayerCompetition(
            Competition.Vttl,
            clubId, uniqueIndex, frenoyLink, ranking, position, rankingIndex, KlassementValueConverter.Vttl(ranking), nextRanking);
    }
}
