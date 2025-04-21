using MongoDB.Driver;
using System.Text;
using System.Text.Json;
using TheGateKeeper.Server.RiotsApiService;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly ILogger<BackgroundWorker> _logger;
        private readonly IMongoCollection<PlayerDaoV1> _playersCollection;
        private readonly IMongoCollection<StoredStandingsDtoV1> _standingsCollection;
        private readonly HttpClient _httpClient;
        private readonly string riotLeagueApi = "https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/";
        private readonly string riotIdByNameAndTag = "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/";
        private readonly string riotSummonerByPuuid = "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/";
        private readonly IRiotApi _riotApi;
        private readonly string _webhookUrl;
        
        public BackgroundWorker(ILogger<BackgroundWorker> logger, IMongoClient mongoClient, IHttpClientFactory httpClientFactory, IConfiguration configuration, IRiotApi riotApi)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            var database = mongoClient.GetDatabase("gateKeeper");
            _playersCollection = database.GetCollection<PlayerDaoV1>("players");
            _standingsCollection = database.GetCollection<StoredStandingsDtoV1>("standings");
            _riotApi = riotApi;
            _webhookUrl = configuration["DiscordWebhook"] ?? configuration["Discord:Webhook"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Checking if PlayerList changed");
                    foreach (var item in CurrentPlayerList.ConstUserList())
                    {
                        var filter = Builders<PlayerDaoV1>.Filter.Where(u => u.UserName == item.name && u.Tag == item.tag);
                        if(_playersCollection.CountDocuments(filter) == 0)
                        {
                            _logger.LogInformation($"Adding new player to database {item.name} #{item.tag}");
                            await AddNewDataBaseEntry(item.name, item.tag);
                        }
                    }
                    var players = await _playersCollection.Find(_ => true).ToListAsync();
                    foreach (var player in players)
                    {
                        var leagueEntries = await GetLeagueEntryListDto(player.Summoner.id);
                        if (leagueEntries.Count() > 0 && leagueEntries.Except(player.LeagueEntries).Count() > 0)
                        {
                            _logger.LogInformation($"Updating entries for {player.UserName}");
                            var filter = Builders<PlayerDaoV1>.Filter.Where(u => u.UserName == player.UserName && u.Tag == player.Tag);
                            var update = Builders<PlayerDaoV1>.Update.Set(m => m.LeagueEntries, leagueEntries);
                            _playersCollection.UpdateOne(filter, update);
                        }
                    }
                    var updatedStandings = await _playersCollection.GetAllRanksFromCollection().ConfigureAwait(false);
                    await CompareStandings(updatedStandings.ToList().FrontEndInfoListToStandings().ToList()).ConfigureAwait(false);

                    _logger.LogInformation($"BackgroundWorker finished process successfully");

                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Data fetch operation cancelled");
                    throw;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error fetching data from API");
                }
                catch (MongoException ex)
                {
                    _logger.LogError(ex, "Error saving data to MongoDB");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error occurred");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task AddNewDataBaseEntry(string userName, string tag)
        {
            var accountDto = await GetAccountDto(userName, tag);
            var summonerDto = await GetSummonerDto(accountDto.puuid);
            var LeagueEntryList = await GetLeagueEntryListDto(summonerDto.id);
            await _playersCollection.InsertOneAsync(new PlayerDaoV1() { 
                UserName = userName, 
                Tag = tag, 
                Account = accountDto, 
                Summoner = summonerDto, 
                Voting = new Voting() { voteBlockedUntil = DateTime.UtcNow, isBlocked = false },
                LeagueEntries = LeagueEntryList 
            }).ConfigureAwait(false);
        }

        private async Task<AccountDtoV1> GetAccountDto(string userName, string tag)
        {
            try
            {
                var url = $"{riotIdByNameAndTag}{userName}/{tag}?api_key={_riotApi.GetCurrentApiKey()}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<RiotErrorCode>();
                    _logger.LogError($"Error during reading of account information: {errorResponse.status.message}");
                    return new AccountDtoV1();
                }
                return await response.Content.ReadFromJsonAsync<AccountDtoV1>().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of account information. Exception {e}");
                return new AccountDtoV1();
            }
        }

        private async Task<SummonerDtoV1> GetSummonerDto(string puuid)
        {
            try
            {

                var url = $"{riotSummonerByPuuid}{puuid}?api_key={_riotApi.GetCurrentApiKey()}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<RiotErrorCode>();
                    _logger.LogError($"Error during reading of summoner info with following error: {errorResponse.status.message}");
                    return new SummonerDtoV1();
                }
                return await response.Content.ReadFromJsonAsync<SummonerDtoV1>().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of summoner information. Exception {e}");
                return new SummonerDtoV1();
            }
        }

        private async Task<List<LeagueEntryDtoV1>> GetLeagueEntryListDto(string id)
        {
            try
            {
                var url = $"{riotLeagueApi}{id}?api_key={_riotApi.GetCurrentApiKey()}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }
                var content = await response.Content.ReadFromJsonAsync<LeagueEntryDtoV1[]>().ConfigureAwait(false);
                return content.ToList();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of league information. Exception {e}");
                throw;
            }
        }

        private async Task CompareStandings(List<Standings> newStandings)
        {
            try
            {
                var oldStandings = await TryGetStandings().ConfigureAwait(false);
                await InsertStandingsTable(newStandings).ConfigureAwait(false);
                if (oldStandings.Count() != newStandings.Count())
                {
                    _logger.LogInformation("The compared standing list have a different amount of users. The comparing process will be skipped once.");
                    return;
                }

                var swaps = new List<(int OriginalIndex, int NewIndex, Standings Item)>();

                // Check each item in the original list
                for (int i = 0; i < oldStandings.Count(); i++)
                {
                    var currentItem = oldStandings[i];
                    int newIndex = newStandings.FindIndex(x => x.name.Equals(currentItem.name));
                    if (newIndex != i && newIndex != -1)
                    {
                        swaps.Add((i, newIndex, currentItem));
                    }
                }

                if (swaps.Count > 0)
                {
                    _logger.LogInformation($"Item {swaps[0].Item.name} moved from position {swaps[0].OriginalIndex} to {swaps[0].NewIndex}");
                    await NotifyDisocrd(swaps).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during comparing of standing lists. Exception {e}");
                throw;
            }
        }
        private async Task InsertStandingsTable(List<Standings> newStandings)
        {
            var filter = Builders<StoredStandingsDtoV1>.Filter.Empty;
            var sort = Builders<StoredStandingsDtoV1>.Sort.Ascending(x => x.Id);
            var update = Builders<StoredStandingsDtoV1>.Update.Set(x => x.Standings, newStandings);

            var options = new FindOneAndUpdateOptions<StoredStandingsDtoV1>
            {
                Sort = sort
            };
            var result = await _standingsCollection.FindOneAndUpdateAsync(filter, update, options);
            if (result is null)
            {
                await _standingsCollection.InsertOneAsync(new StoredStandingsDtoV1() { Id = "standingstable", Standings = newStandings });
            }
        }

        private async Task<List<Standings>> TryGetStandings()
        {
            try
            {
                var standingsObject =  await _standingsCollection.Find(_ => true).FirstAsync();
                return standingsObject.Standings;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during finding the standings table {e}");
                throw;
            }
        }

        private async Task NotifyDisocrd(List<(int OriginalIndex, int NewIndex, Standings Item)> swappedPlayers)
        {
            var returnMessage = "";
            foreach (var (OriginalIndex, NewIndex, Item) in swappedPlayers)
            {
                returnMessage += $"{Item.name} moved to new position {NewIndex} \n";
            }
            var payload = new
            {
                content = $"There was a change to the gate keeper rankings.\n{returnMessage}",
                username = "The Gate Keeper"
            };

            var json = JsonSerializer.Serialize(payload);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync(_webhookUrl, contentData);
        }
    }
}
