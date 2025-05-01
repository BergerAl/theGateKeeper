using System.Text.Json.Serialization;

namespace TheGateKeeper.Server
{
    public class AccountDtoV1
    {
        public string puuid { get; set; } = string.Empty;
        public string gameName { get; set; } = string.Empty;
        public string tagLine { get; set; } = string.Empty;
    }

    public class SummonerDtoV1
    {
        public string accountId { get; set; } = string.Empty;
        public int profileIconId { get; set; } = 0;
        public float revisionDate { get; set; } = 0;
        public string id { get; set; } = string.Empty;
        public string puuid { get; set; } = string.Empty;
        public float summonerLevel { get; set; } = 0;
    }

    public class SpectatorDtoV1 
    {
        public float gameId { get; set; } = 0;
        public string gameMode {  get; set; } = string.Empty;
    }

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

    public class VotingCallResult
    {
        public bool Success;
        public string? ErrorMessage = "";
    }

    public class Standings
    {
        public string name { get; set; } = string.Empty;
        public string tier { get; set; } = string.Empty;
        public string rank { get; set; } = string.Empty;
        public int leaguePoints { get; set; } = 0;
        public int playedGames { get; set; } = 0;
    }

    public class FrontEndInfo : Standings
    {
        public VotingDtoV1 voting { get; set; } = new VotingDtoV1() { isBlocked = false, voteBlockedUntil = DateTime.UtcNow };
    }

    public class VotingStandingsDtoV1
    {
        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; } = string.Empty;
        [JsonPropertyName("votes")]
        public double Votes { get; set; } = 0;
    }

    public class RiotUser
    {
        public string Name { get; set; } = "";
        public string Tag { get; set; } = "";
    }

    public class RiotErrorCode
    {
        [JsonPropertyName("status")]
        public RiotInnerErrorCode Status { get; set; }
    }

    public class RiotInnerErrorCode
    {
        public double status_code { get; set; }
        public string message { get; set; }
    }
}
