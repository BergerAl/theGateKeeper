using AutoMapper;
using TheGateKeeper.Server;

public class VotingProfile : Profile
{
    public VotingProfile()
    {
        CreateMap<VotingDaoV1, VotingDtoV1>()
            .ForMember(dest => dest.isBlocked, opt => opt.MapFrom(src => src.isBlocked))
            .ForMember(dest => dest.voteBlockedUntil, opt => opt.MapFrom(src => src.voteBlockedUntil));

        CreateMap<VotingDtoV1, VotingDaoV1>()
            .ForMember(dest => dest.countAmount, opt => opt.UseDestinationValue());
    }
}