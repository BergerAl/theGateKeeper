using AutoMapper;
using TheGateKeeper.Server;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<VotingDaoV1, VotingDtoV1>()
            .ForMember(dest => dest.isBlocked, opt => opt.MapFrom(src => src.isBlocked))
            .ForMember(dest => dest.voteBlockedUntil, opt => opt.MapFrom(src => src.voteBlockedUntil));

        CreateMap<VotingDtoV1, VotingDaoV1>()
            .ForMember(dest => dest.countAmount, opt => opt.UseDestinationValue());

        CreateMap<AppConfigurationDaoV1, AppConfigurationDtoV1>()
            .ForMember(dest => dest.DisplayedView, opt => opt.MapFrom(src => src.DisplayedView));

        CreateMap<AppConfigurationDtoV1, AppConfigurationDaoV1>()
            .ForMember(dest => dest.Id, opt => opt.UseDestinationValue());

        CreateMap<GateKeeperInformationDaoV1, GateKeeperInformationDtoV1>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.GameId));

        CreateMap<GateKeeperInformationDtoV1, GateKeeperInformationDaoV1>()
            .ForMember(dest => dest.Id, opt => opt.UseDestinationValue());
    }
}