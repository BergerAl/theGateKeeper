using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class SummonerDtoV1
{
    public string accountId { get; set; } = string.Empty;
    public int profileIconId { get; set; } = 0;
    public float revisionDate { get; set; } = 0;
    public string id { get; set; } = string.Empty;
    public string puuid { get; set; } = string.Empty;
    public float summonerLevel { get; set; } = 0;
}
