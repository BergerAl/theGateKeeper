using System.Text.Json.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class GateKeeperInformationDtoV1
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}