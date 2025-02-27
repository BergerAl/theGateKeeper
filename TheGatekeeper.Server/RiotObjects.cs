namespace TheGateKeeper.Server
{
    public class AccountDto
    {
        public string puuid { get; set; } = string.Empty;
        public string gameName { get; set; } = string.Empty;
        public string tagLine { get; set; } = string.Empty;
    }

    public class SummonerDto
    {
        public string accountId { get; set; } = string.Empty;
        public int profileIconId { get; set; } = 0;
        public float revisionDate { get; set; } = 0;
        public string id { get; set; } = string.Empty;
        public string puuid { get; set; } = string.Empty;
        public float summonerLevel { get; set; } = 0;
    }

    public class LeagueEntryDto
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
    }

    public class FrontEndInfo
    {
        public string name { get; set; } = string.Empty;
        public string tier { get; set; } = string.Empty;
        public string rank { get; set; } = string.Empty;
        public int leaguePoints { get; set; } = 0;
        public int playedGames { get; set; } = 0;
    }

    public class RiotUser
    {
        public string name { get; set; }
        public string tag { get; set; }
    }
}
