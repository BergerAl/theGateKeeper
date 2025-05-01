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
    }
}