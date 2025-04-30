using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;

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

        public Voting Voting { get; set; }
        public List<LeagueEntryDtoV1> LeagueEntries { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Voting
    {
        public double countAmount { get; set; }
        public bool isBlocked { get; set; }
        public DateTime voteBlockedUntil { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class StoredStandingsDtoV1
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public List<Standings> Standings { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AppConfigurationDtoV1
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DisplayedView DisplayedView { get; set; }
    }

    public enum DisplayedView
    {
        DefaultPage,
        ResultsPage
    }
}
