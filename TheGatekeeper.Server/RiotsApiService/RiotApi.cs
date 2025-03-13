using MongoDB.Driver;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;

        public RiotApi(IMongoClient mongoClient, ILogger<RiotApi> logger) {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _logger = logger;
        }

        public async Task<IEnumerable<FrontEndInfo>> GetAllRanks()
        {
            try
            {
                var responseList = new List<FrontEndInfo>();
                var players = await _collection.Find(_ => true).ToListAsync();
                foreach (var player in players)
                {
                    var element = player.LeagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5").First();
                    var frontEndInfo = new FrontEndInfo()
                    {
                        leaguePoints = element.leaguePoints,
                        name = player.UserName,
                        rank = element.rank,
                        tier = element.tier,
                        playedGames = element.wins + element.losses
                    };
                    responseList.Add(frontEndInfo);
                }
                responseList = SortUsers(responseList);
                return responseList;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of users. Exception {e}");
                return [new FrontEndInfo()];
            }

        }

        private List<FrontEndInfo> SortUsers(IEnumerable<FrontEndInfo> users)
        {
            var customTierRank = new Dictionary<string, int>
                {
                    {"PLATINUM", 1},
                    {"GOLD", 2},
                    {"SILVER", 3},
                    {"BRONZE", 4},

            };
            var customRankRank = new Dictionary<string, int>
                {
                    {"I", 1},
                    {"II", 2},
                    {"III", 3},
                    {"IV", 4},

            };
            return users.OrderBy(x =>
                customTierRank.ContainsKey(x.tier) ? customTierRank[x.tier] : int.MaxValue).ThenBy(x =>
                customRankRank.ContainsKey(x.rank) ? customRankRank[x.rank] : int.MaxValue).ThenByDescending(x => x.leaguePoints).ToList();
        }
    }
}
