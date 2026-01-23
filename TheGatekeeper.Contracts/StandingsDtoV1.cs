using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class StandingsDtoV1
{
    public string name { get; set; } = string.Empty;
    public string tier { get; set; } = string.Empty;
    public string rank { get; set; } = string.Empty;
    public int leaguePoints { get; set; } = 0;
    public int playedGames { get; set; } = 0;
}
