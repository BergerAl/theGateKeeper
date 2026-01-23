using System.Text.Json.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class GateKeeperAppInfoDtoV1
{
    [JsonPropertyName("usersOnline")]
    public int UsersOnline { get; set; } = 0;
}
