using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class VotingCallResultDtoV1
{
    public bool Success;
    public string? ErrorMessage = "";
}
