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

    [JsonPropertyName("votingEndsAt")]
    public DateTime? VotingEndsAt { get; set; }

    [JsonPropertyName("voteBlockCooldownSeconds")]
    public double VoteBlockCooldownSeconds { get; set; } = 0.5;

    [JsonPropertyName("enabledTabs")]
    public List<string> EnabledTabs { get; set; } = ["LeagueStandings", "Results", "Users", "UserVotings"];
}
