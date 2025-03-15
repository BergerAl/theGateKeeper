using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;

namespace TheGateKeeper.Server
{
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

    public class Voting
    {
        public bool isBlocked { get; set; }
        public DateTime voteBlockedUntil { get; set; }
    }

    public class StoredStandingsDtoV1
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }
        public List<Standings> Standings { get; set; }
    }
}
