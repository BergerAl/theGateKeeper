using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class SpectatorDtoV1
{
    public float gameId { get; set; } = 0;
    public string gameMode { get; set; } = string.Empty;
}
