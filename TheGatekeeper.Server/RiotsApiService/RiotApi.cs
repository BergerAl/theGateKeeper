using System.Text.Json;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string riotIdByNameAndTag = "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/";
        private readonly string riotSummonerByPuuid = "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/";
        private readonly string riotLeagueApi = "https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/";

        public RiotApi(IHttpClientFactory httpClientFactory, ILogger<RiotApi> logger) {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<FrontEndInfo>> GetAllRanks(string apiKey)
        {
            try
            {
                if (apiKey == null) {
                    var apiFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../api_key");
                    apiKey = File.ReadAllText(apiFilePath);
                }
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
                        var errorResponse = await response.Content.ReadFromJsonAsync<RiotErrorCode>();
                        _logger.LogError($"Error during reading of user info with following error: {errorResponse.status.message}");
                        return [new FrontEndInfo()];
                    }
                    var data = await response.Content.ReadAsStringAsync();
                    var accountDto = JsonSerializer.Deserialize<AccountDtoV1>(data);

                    url = $"{riotSummonerByPuuid}{accountDto.puuid}?api_key={apiKey}";
                    var summonerResponse = await httpClient.GetAsync(url);
                    if (!summonerResponse.IsSuccessStatusCode)
                    {
                        return [new FrontEndInfo()];
                    }
                    data = await summonerResponse.Content.ReadAsStringAsync();
                    var summonerDto = JsonSerializer.Deserialize<SummonerDtoV1>(data);

                    url = $"{riotLeagueApi}{summonerDto.id}?api_key={apiKey}";
                    var leagueResponse = await httpClient.GetAsync(url);
                    if (!leagueResponse.IsSuccessStatusCode)
                    {
                        return [new FrontEndInfo()];
                    }
                    data = await leagueResponse.Content.ReadAsStringAsync();
                    var leagueEntryDto = JsonSerializer.Deserialize<LeagueEntryDtoV1[]>(data);
                    var element = leagueEntryDto.Where(x => x.queueType == "RANKED_SOLO_5x5").First();
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
                new RiotUser() {
                    name = "DönerBoxSchmaus",
                    tag = "EUW"
                },
            ];
        }
    }
}
