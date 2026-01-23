using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class VotingDtoV1
{
    public bool isBlocked { get; set; }
    public DateTime voteBlockedUntil { get; set; }
}
