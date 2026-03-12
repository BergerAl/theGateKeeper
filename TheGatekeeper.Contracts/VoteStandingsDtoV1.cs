using System.Text.Json.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class VoteStandingsDtoV1
{
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;
    
    [JsonPropertyName("votes")]
    public double Votes { get; set; } = 0;
}
