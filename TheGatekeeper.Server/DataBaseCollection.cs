using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Text.Json.Serialization;

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
    }

    [BsonIgnoreExtraElements]
    public class VotingDaoV1
    {
        public double countAmount { get; set; }
        public bool isBlocked { get; set; }
        public DateTime voteBlockedUntil { get; set; }
    }

    public class VotingDtoV1
    {
        public bool isBlocked { get; set; }
        public DateTime voteBlockedUntil { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class StoredStandingsDaoV1
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public List<Standings> Standings { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AppConfigurationDaoV1
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DisplayedView DisplayedView { get; set; }
    }

    public class AppConfigurationDtoV1
    {
        [JsonPropertyName("displayedView")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DisplayedView DisplayedView { get; set; }
    }

    public enum DisplayedView
    {
        DefaultPage,
        ResultsPage
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
    public class GateKeeperInformationDtoV1
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("gameId")]
        public float GameId { get; set; } = 0;
    }
}
