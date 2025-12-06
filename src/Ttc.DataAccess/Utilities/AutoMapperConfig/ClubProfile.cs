using System.Collections.ObjectModel;
using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model.Clubs;

namespace Ttc.DataAccess.Utilities.AutoMapperConfig;

internal class ClubProfile : Profile
{
    public ClubProfile()
    {
        CreateMap<ClubEntity, Club>()
            .ForMember(
                dest => dest.Name,
                opts => opts.MapFrom(src => src.Name))
            .ForMember(
                dest => dest.Active,
                opts => opts.MapFrom(src => src.Active))
            .ForMember(
                dest => dest.Shower,
                opts => opts.MapFrom(src => src.Shower))
            .ForMember(
                dest => dest.MainLocation,
                opts => opts.MapFrom(src => CreateMainClubLocation(src.Locations)))
            .ForMember(
                dest => dest.AlternativeLocations,
                opts => opts.MapFrom(src => CreateSecondaryClubLocations(src.Locations)))
            .ForMember(dest => dest.Managers, opts => opts.Ignore());
    }

    private static ICollection<Model.Clubs.ClubLocation> CreateSecondaryClubLocations(ICollection<ClubLocationEntity> allLocations)
    {
        var locations = allLocations.Where(x => !x.MainLocation).ToArray();
        if (!locations.Any())
        {
            return new Collection<Model.Clubs.ClubLocation>();
        }
        return locations.Select(CreateClubLocation).ToArray();
    }

    private static Model.Clubs.ClubLocation CreateMainClubLocation(ICollection<ClubLocationEntity> locations)
    {
        var mainLocation = locations.FirstOrDefault(x => x.MainLocation);
        if (mainLocation == null)
        {
            return new Model.Clubs.ClubLocation();
        }
        return CreateClubLocation(mainLocation);
    }

    private static Model.Clubs.ClubLocation CreateClubLocation(ClubLocationEntity location)
    {
        return new Model.Clubs.ClubLocation
        {
            Id = location.Id,
            Description = location.Description,
            Address = location.Address,
            PostalCode = location.PostalCode.ToString(),
            City = location.City,
            Mobile = location.Mobile,
            Comment = location.Comment ?? "",
        };
    }
}
