using MongoDB.Driver;

namespace TheGateKeeper.Server
{
    public static class CommonMethods
    {
        public static IEnumerable<FrontEndInfo> SortUsers(this IEnumerable<FrontEndInfo> users)
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

        public static async Task<IEnumerable<FrontEndInfo>> GetAllRanksFromCollection(this IMongoCollection<PlayerDaoV1> collection)
        {
            var responseList = new List<FrontEndInfo>();
            var players = await collection.Find(_ => true).ToListAsync();
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
            return responseList.SortUsers();
        }
    }
}
