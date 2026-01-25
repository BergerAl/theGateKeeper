using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Text.Json.Serialization;
using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server
{
    [BsonIgnoreExtraElements]
    public class PlayerDaoV1
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public string UserName { get; set; }

        public string Tag { get; set; }
        public AccountDtoV1 Account { get; set; }
        public SummonerDtoV1 Summoner { get; set; }

        public VotingDaoV1 Voting { get; set; }
        public List<LeagueEntryDtoV1> LeagueEntries { get; set; }
        public StoredMatchDaoV1 StoredLastMatch { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class VotingDaoV1
    {
        public double countAmount { get; set; }
        public bool isBlocked { get; set; }
        public DateTime voteBlockedUntil { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class StoredStandingsDaoV1
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public List<StandingsDtoV1> Standings { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AppConfigurationDaoV1
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DisplayedView DisplayedView { get; set; }
        public bool VotingDisabled { get; set; }
        public bool DisplayResultsBar { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class GateKeeperInformationDaoV1
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public string Name { get; set; } = "";
        public float GameId { get; set; } = 0;

    }

    [BsonIgnoreExtraElements]
    public class RankTimeLineEntryDaoV1
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = "";
        public List<RankTimeLineDaoV1> RankTimeLine { get; set; } = [];
    }

    [BsonIgnoreExtraElements]
    public class RankTimeLineDaoV1
    {
        public DateTime DateTime { get; set; }
        public int CombinedPoints { get; set; } = 0;
    }

    [BsonIgnoreExtraElements]
    public class StoredMatchDaoV1
    {
        [JsonPropertyName("matchId")]
        public string MatchId { get; set; } = "";
        public string PlayerPuuid { get; set; } = "";
        public DateTime StoredAt { get; set; }
        public long GameCreation { get; set; }
        public int GameDuration { get; set; }
        public string GameMode { get; set; } = "";
        public string GameType { get; set; } = "";
        public int MapId { get; set; }
        public int QueueId { get; set; }
        public int ChampionId { get; set; }
        public string ChampionName { get; set; } = "";
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public bool Win { get; set; }
        public int GoldEarned { get; set; }
        public int TotalDamageDealtToChampions { get; set; }
        public int TotalDamageTaken { get; set; }
        public int ChampLevel { get; set; }
        public int Item0 { get; set; }
        public int Item1 { get; set; }
        public int Item2 { get; set; }
        public int Item3 { get; set; }
        public int Item4 { get; set; }
        public int Item5 { get; set; }
        public int Item6 { get; set; }
    }
}
