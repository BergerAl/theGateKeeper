using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server
{
    public static class CommonMethods
    {
        public static IEnumerable<FrontEndInfoDtoV1> SortUsers(this IEnumerable<FrontEndInfoDtoV1> users)
        {
            var customTierRank = new Dictionary<string, int>
                {
                    {"CHALLENGER", 1},
                    {"GRANDMASTER", 2},
                    {"MASTER", 3},
                    {"DIAMOND", 4},
                    {"EMERALD", 5},
                    {"PLATINUM", 6},
                    {"GOLD", 7},
                    {"SILVER", 8},
                    {"BRONZE", 9},

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

        public static async Task<IEnumerable<FrontEndInfoDtoV1>> GetAllRanksFromCollection(this IMongoCollection<PlayerDaoV1> collection, IMapper mapper, ILogger logger)
        {       
            var players = await collection.Find(_ => true).ToListAsync();
            return players.PlayerToFrontEndInfo(mapper, logger).SortUsers();
        }

        public static IEnumerable<FrontEndInfoDtoV1> PlayerToFrontEndInfo(this List<PlayerDaoV1> players, IMapper mapper, ILogger logger)
        {
            try {
                var responseList = new List<FrontEndInfoDtoV1>();
                foreach (var player in players)
                {
                    var element = player.LeagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5")?.FirstOrDefault();
                    if (element == null)
                    {
                        logger.LogInformation($"No RANKED_SOLO_5x5 entry found for player {player.UserName}");
                        var defaultFrontEndInfo = new FrontEndInfoDtoV1()
                        {
                            leaguePoints = 0,
                            name = player.UserName,
                            rank = "Loves his wood",
                            tier = "UNRANKED",
                            playedGames = 0,
                            voting = mapper.Map<VotingDtoV1>(player.Voting)
                        };
                        responseList.Add(defaultFrontEndInfo);
                        continue;
                    }
                    var frontEndInfo = new FrontEndInfoDtoV1()
                    {
                        leaguePoints = element.leaguePoints,
                        name = player.UserName,
                        rank = element.rank,
                        tier = element.tier,
                        playedGames = element.wins + element.losses,
                        voting = mapper.Map<VotingDtoV1>(player.Voting)
                    };
                    responseList.Add(frontEndInfo);
                }
                return responseList;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in PlayerToFrontEndInfo: {ex.Message}. StackTrace: {ex.StackTrace}");
                return new List<FrontEndInfoDtoV1>();
            }

        }

        public static IEnumerable<FrontEndInfoDtoV1> PlayerToFrontEndInfoUnblocked(this List<PlayerDaoV1> players)
        {
            var responseList = new List<FrontEndInfoDtoV1>();
            foreach (var player in players)
            {
                var element = player.LeagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5").First();
                var frontEndInfo = new FrontEndInfoDtoV1()
                {
                    leaguePoints = element.leaguePoints,
                    name = player.UserName,
                    rank = element.rank,
                    tier = element.tier,
                    playedGames = element.wins + element.losses,
                    voting = new VotingDtoV1() { isBlocked = false, voteBlockedUntil = player.Voting.voteBlockedUntil }
                };
                responseList.Add(frontEndInfo);
            }
            return responseList;
        }

        public static IEnumerable<StandingsDtoV1> FrontEndInfoListToStandings(this List<FrontEndInfoDtoV1> info)
        {
            var responseList = new List<StandingsDtoV1>();
            foreach (var player in info)
            {
                var frontEndInfo = new StandingsDtoV1()
                {
                    leaguePoints = player.leaguePoints,
                    name = player.name,
                    rank = player.rank,
                    tier = player.tier,
                    playedGames = player.playedGames
                };
                responseList.Add(frontEndInfo);
            }
            return responseList;
        }
    }
}
