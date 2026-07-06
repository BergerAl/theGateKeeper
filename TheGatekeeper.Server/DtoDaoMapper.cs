using Riok.Mapperly.Abstractions;
using TheGateKeeper.Server;
using TheGatekeeper.Contracts;

[Mapper]
public partial class DtoMapper
{
    public partial VotingDtoV1 ToDto(VotingDaoV1 source);

    public partial AppConfigurationDtoV1 ToDto(AppConfigurationDaoV1 source);

    [MapperIgnoreTarget(nameof(AppConfigurationDaoV1.Id))]
    public partial AppConfigurationDaoV1 ToDao(AppConfigurationDtoV1 source);

    public partial GateKeeperInformationDtoV1 ToDto(GateKeeperInformationDaoV1 source);
}