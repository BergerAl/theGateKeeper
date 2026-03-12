using TypeGen.Core.TypeAnnotations;

namespace TheGatekeeper.Contracts;

[ExportTsInterface]
public class LeagueEntryDtoV1 : IEquatable<LeagueEntryDtoV1>
{
    public string leagueId { get; set; } = string.Empty;
    public string summonerId { get; set; } = string.Empty;
    public string queueType { get; set; } = string.Empty;
    public string tier { get; set; } = string.Empty;
    public string rank { get; set; } = string.Empty;
    public int leaguePoints { get; set; } = 0;
    public int wins { get; set; } = 0;
    public int losses { get; set; } = 0;
    public bool hotStreak { get; set; } = false;
    public bool veteran { get; set; } = false;
    public bool freshBlood { get; set; } = false;
    public bool inactive { get; set; } = false;

    public bool Equals(LeagueEntryDtoV1? other)
    {
        if (other is null) return false;
        return leagueId == other.leagueId &&
            summonerId == other.summonerId &&
            queueType == other.queueType &&
            tier == other.tier &&
            rank == other.rank &&
            leaguePoints == other.leaguePoints &&
            wins == other.wins &&
            losses == other.losses &&
            hotStreak == other.hotStreak &&
            veteran == other.veteran &&
            freshBlood == other.freshBlood &&
            inactive == other.inactive;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(leagueId, summonerId, queueType, tier, rank, leaguePoints, wins, losses);
    }
}
