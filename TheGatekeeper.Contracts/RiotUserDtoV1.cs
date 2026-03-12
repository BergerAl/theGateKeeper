using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class RiotUserDtoV1
{
    public string Name { get; set; } = "";
    public string Tag { get; set; } = "";
}
