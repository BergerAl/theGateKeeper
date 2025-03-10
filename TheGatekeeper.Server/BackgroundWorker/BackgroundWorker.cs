using MongoDB.Driver;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly ILogger<BackgroundWorker> _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;
        private readonly HttpClient _httpClient;
        private readonly string riotLeagueApi = "https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/";
        private readonly string riotIdByNameAndTag = "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/";
        private readonly string riotSummonerByPuuid = "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/";
        private readonly string _apiKey;
        
        public BackgroundWorker(ILogger<BackgroundWorker> logger, IMongoClient mongoClient, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _apiKey = configuration["api_key"] ?? File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../api_key"));
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
                        if(_collection.CountDocuments(filter) == 0)
                        {
                            _logger.LogInformation($"Adding new player to database {item.name} #{item.tag}");
                            await AddNewDataBaseEntry(item.name, item.tag);
                        }
                    }
                    var players = await _collection.Find(_ => true).ToListAsync();
                    foreach (var player in players)
                    {
                        var leagueEntries = await GetLeagueEntryListDto(player.Summoner.id);
                        if (leagueEntries.Count() > 0 && leagueEntries.Except(player.LeagueEntries).Count() > 0)
                        {
                            _logger.LogInformation($"Updating entries for {player.UserName}");
                            var filter = Builders<PlayerDaoV1>.Filter.Where(u => u.UserName == player.UserName && u.Tag == player.Tag);
                            var update = Builders<PlayerDaoV1>.Update.Set(m => m.LeagueEntries, leagueEntries);
                            _collection.UpdateOne(filter, update);
                        }
                    }
                    _logger.LogInformation($"BackgorundWorker finished process successfully");

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
            await _collection.InsertOneAsync(new PlayerDaoV1() { UserName = userName, Tag = tag, Account = accountDto, Summoner = summonerDto, LeagueEntries = LeagueEntryList }).ConfigureAwait(false);
        }

        private async Task<AccountDtoV1> GetAccountDto(string userName, string tag)
        {
            try
            {
                var url = $"{riotIdByNameAndTag}{userName}/{tag}?api_key={_apiKey}";
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

                var url = $"{riotSummonerByPuuid}{puuid}?api_key={_apiKey}";
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
                var url = $"{riotLeagueApi}{id}?api_key={_apiKey}";
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
    }
}
