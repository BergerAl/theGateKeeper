﻿using AutoMapper;
using MongoDB.Driver;

namespace TheGateKeeper.Server
{
    public static class CommonMethods
    {
        public static IEnumerable<FrontEndInfo> SortUsers(this IEnumerable<FrontEndInfo> users)
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

        public static async Task<IEnumerable<FrontEndInfo>> GetAllRanksFromCollection(this IMongoCollection<PlayerDaoV1> collection, IMapper mapper)
        {       
            var players = await collection.Find(_ => true).ToListAsync();
            return players.PlayerToFrontEndInfo(mapper).SortUsers();
        }

        public static IEnumerable<FrontEndInfo> PlayerToFrontEndInfo(this List<PlayerDaoV1> players, IMapper mapper)
        {
            var responseList = new List<FrontEndInfo>();
            foreach (var player in players)
            {
                var element = player.LeagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5").First();
                var frontEndInfo = new FrontEndInfo()
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

        public static IEnumerable<FrontEndInfo> PlayerToFrontEndInfoUnblocked(this List<PlayerDaoV1> players)
        {
            var responseList = new List<FrontEndInfo>();
            foreach (var player in players)
            {
                var element = player.LeagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5").First();
                var frontEndInfo = new FrontEndInfo()
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

        public static IEnumerable<Standings> FrontEndInfoListToStandings(this List<FrontEndInfo> info)
        {
            var responseList = new List<Standings>();
            foreach (var player in info)
            {
                var frontEndInfo = new Standings()
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
