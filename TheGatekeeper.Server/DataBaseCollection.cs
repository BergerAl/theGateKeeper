using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace TheGateKeeper.Server
{
    public class PlayerDaoV1
    {
        //[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.String)]
        public string? UserName { get; set; }

        public string Tag { get; set; }
        public AccountDtoV1 Account { get; set; }
        public SummonerDtoV1 Summoner { get; set; }
        public LeagueEntryDtoV1 LeagueEntry { get; set; }
    }
}
