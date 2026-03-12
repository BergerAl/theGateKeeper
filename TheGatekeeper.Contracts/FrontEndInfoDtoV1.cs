using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class FrontEndInfoDtoV1 : StandingsDtoV1
{
    public VotingDtoV1 voting { get; set; } = new VotingDtoV1() { isBlocked = false, voteBlockedUntil = DateTime.UtcNow };
}
