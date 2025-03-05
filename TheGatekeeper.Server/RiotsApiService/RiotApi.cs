using System.Text.Json;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string riotIdByNameAndTag = "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/";
        private readonly string riotSummonerByPuuid = "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/";
        private readonly string riotLeagueApi = "https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/";

        public RiotApi(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<FrontEndInfo>> GetAllRanks(string apiKey)
        {
            try
            {
                var responseList = new List<FrontEndInfo>();
                using var httpClient = _httpClientFactory.CreateClient();
                foreach (var item in ConstUserList())
                {
                    string userName = Uri.EscapeDataString(item.name);
                    string tag = item.tag;
                    var url = $"{riotIdByNameAndTag}{userName}/{tag}?api_key={apiKey}";
                    var response = await httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        return [new FrontEndInfo()];
                    }
                    var data = await response.Content.ReadAsStringAsync();
                    var accountDto = JsonSerializer.Deserialize<AccountDto>(data);

                    url = $"{riotSummonerByPuuid}{accountDto.puuid}?api_key={apiKey}";
                    var summonerResponse = await httpClient.GetAsync(url);
                    if (!summonerResponse.IsSuccessStatusCode)
                    {
                        return [new FrontEndInfo()];
                    }
                    data = await summonerResponse.Content.ReadAsStringAsync();
                    var summonerDto = JsonSerializer.Deserialize<SummonerDto>(data);

                    url = $"{riotLeagueApi}{summonerDto.id}?api_key={apiKey}";
                    var leagueResponse = await httpClient.GetAsync(url);
                    if (!leagueResponse.IsSuccessStatusCode)
                    {
                        return [new FrontEndInfo()];
                    }
                    data = await leagueResponse.Content.ReadAsStringAsync();
                    var leagueEntryDto = JsonSerializer.Deserialize<LeagueEntryDto[]>(data);
                    var element = leagueEntryDto.First();
                    var frontEndInfo = new FrontEndInfo()
                    {
                        leaguePoints = element.leaguePoints,
                        name = item.name,
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

        private IEnumerable<RiotUser> ConstUserList()
        {
            return [
                new RiotUser() {
                    name = "Knechter",
                    tag = "EUW"
                },
                new RiotUser() {
                    name = "Major Hefeweizen",
                    tag = "xdd"
                },
                new RiotUser() {
                    name = "Tabatschko",
                    tag = "EUW"
                },
                new RiotUser() {
                    name = "Oizo",
                    tag = "Luxi"
                },
                new RiotUser() {
                    name = "LuXi",
                    tag = "Oizo"
                },
                new RiotUser() {
                    name = "Blinded Manu",
                    tag = "3532"
                },
                new RiotUser() {
                    name = "Hobbobbelmobmob",
                    tag = "EUW"
                },
                new RiotUser() {
                    name = "Joizo",
                    tag = "EUW"
                },
            ];
        }
    }
}
