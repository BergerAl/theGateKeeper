using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class AccountDtoV1
{
    public string puuid { get; set; } = string.Empty;
    public string gameName { get; set; } = string.Empty;
    public string tagLine { get; set; } = string.Empty;
}
