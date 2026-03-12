using System.Text.Json.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class AppConfigurationDtoV1
{
    [JsonPropertyName("displayedView")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DisplayedView DisplayedView { get; set; }

    [JsonPropertyName("votingDisabled")]
    public bool VotingDisabled { get; set; }

    [JsonPropertyName("displayResultsBar")]
    public bool DisplayResultsBar { get; set; }
}
