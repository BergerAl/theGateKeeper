using AutoMapper;
using MongoDB.Driver;
using System.Text;
using System.Text.Json;
using TheGateKeeper.Controllers;
using TheGateKeeper.Server.RiotsApiService;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly ILogger<BackgroundWorker> _logger;
        private readonly IMongoCollection<PlayerDaoV1> _playersCollection;
        private readonly IMongoCollection<StoredStandingsDaoV1> _standingsCollection;
        private readonly IMongoCollection<GateKeeperInformationDaoV1> _gateKeeperCollection;
        private readonly IMongoCollection<RankTimeLineEntryDaoV1> _historyCollection;
        private readonly HttpClient _httpClient;
        private readonly string riotLeagueApi = "https://euw1.api.riotgames.com/lol/league/v4/entries/by-puuid/";
        private readonly string riotIdByNameAndTag = "https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/";
        private readonly string riotSummonerByPuuid = "https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-puuid/";
        private readonly string riotSpectatorId = "https://euw1.api.riotgames.com/lol/spectator/v5/active-games/by-summoner/";
        private readonly IRiotApi _riotApi;
        private readonly string _webhookUrl;
        private readonly IMapper _mapper;
        
        public BackgroundWorker(ILogger<BackgroundWorker> logger, IMongoClient mongoClient, IHttpClientFactory httpClientFactory, IConfiguration configuration, IRiotApi riotApi, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            var database = mongoClient.GetDatabase("gateKeeper");
            _playersCollection = database.GetCollection<PlayerDaoV1>("players");
            _standingsCollection = database.GetCollection<StoredStandingsDaoV1>("standings");
            _gateKeeperCollection = database.GetCollection<GateKeeperInformationDaoV1>("gateKeeperInfo");
            _historyCollection = database.GetCollection<RankTimeLineEntryDaoV1>("ranktimeline");
            _riotApi = riotApi;
            _webhookUrl = SecretsHelper.GetSecret(configuration, "discordWebhook");
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
                        var filter = Builders<PlayerDaoV1>.Filter.Where(u => u.UserName == item.Name && u.Tag == item.Tag);
                        if(_playersCollection.CountDocuments(filter) == 0)
                        {
                            _logger.LogDebug($"Adding new player to database {item.Name} #{item.Tag}");
                            await AddNewDataBaseEntry(item.Name, item.Tag, stoppingToken);
                        }
                    }
                    var players = await _playersCollection.Find(_ => true).ToListAsync();
                    foreach (var player in players)
                    {
                        var leagueEntries = await GetLeagueEntryListDto(player.Summoner.puuid, stoppingToken);
                        if (leagueEntries.Count > 0 && leagueEntries.Except(player.LeagueEntries).Count() > 0)
                        {
                            _logger.LogDebug($"Updating entries for {player.UserName}");
                            var filter = Builders<PlayerDaoV1>.Filter.Where(u => u.UserName == player.UserName && u.Tag == player.Tag);
                            var update = Builders<PlayerDaoV1>.Update.Set(m => m.LeagueEntries, leagueEntries);
                            _playersCollection.UpdateOne(filter, update);
                        }
                        if (leagueEntries.Count > 0)
                        {
                            _logger.LogDebug($"Updating rank time line entries for {player.UserName}");
                            var historyFilter = Builders<RankTimeLineEntryDaoV1>.Filter.Where(u => u.UserName == player.UserName);
                            var historyEntry = await _historyCollection.Find(historyFilter).FirstOrDefaultAsync();
                            var currentTier = leagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5").First().tier;
                            var currentRank = leagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5").First().rank;
                            var currentLeaguePoints = leagueEntries.Where(x => x.queueType == "RANKED_SOLO_5x5").First().leaguePoints;

                            var rankTimeLine = new RankTimeLineDaoV1()
                            {
                                DateTime = DateTime.UtcNow,
                                CombinedPoints = GetCombinedPoints(currentTier, currentRank, currentLeaguePoints),
                            };

                            if (historyEntry == null)
                            {
                                // Insert new entry if not exists
                                var newEntry = new RankTimeLineEntryDaoV1
                                {
                                    UserName = player.UserName,
                                    RankTimeLine = [rankTimeLine]
                                };
                                await _historyCollection.InsertOneAsync(newEntry);
                            }
                            else
                            {
                                var lastEntry = historyEntry.RankTimeLine.Last();

                                if (lastEntry.CombinedPoints != GetCombinedPoints(currentTier, currentRank, currentLeaguePoints) || lastEntry.DateTime.Date != DateTime.UtcNow.Date)
                                {
                                    // Update existing entry
                                    var historyUpdate = Builders<RankTimeLineEntryDaoV1>.Update.AddToSet(
                                        entry => entry.RankTimeLine, rankTimeLine
                                    );
                                    _historyCollection.UpdateOne(historyFilter, historyUpdate);
                                }
                            }
                        }
                    }
                    await NotifyDiscordGateKeeperPlaying(stoppingToken);
                    var updatedStandings = await _playersCollection.GetAllRanksFromCollection(_mapper).ConfigureAwait(false);
                    await CompareStandings(updatedStandings.ToList().FrontEndInfoListToStandings().ToList(), stoppingToken).ConfigureAwait(false);

                    _logger.LogInformation($"BackgroundWorker finished process successfully");

                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning(ex, "HTTP request timed out or was canceled");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Data fetch operation cancelled");
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

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("BackgroundWorker is shutting down");
                    break;
                }
            }
        }

        private async Task AddNewDataBaseEntry(string userName, string tag, CancellationToken stoppingToken)
        {
            var accountDto = await GetAccountDto(userName, tag, stoppingToken);
            var summonerDto = await GetSummonerDto(accountDto.puuid, stoppingToken);
            var LeagueEntryList = await GetLeagueEntryListDto(summonerDto.id, stoppingToken);
            await _playersCollection.InsertOneAsync(new PlayerDaoV1() { 
                UserName = userName, 
                Tag = tag, 
                Account = accountDto, 
                Summoner = summonerDto, 
                Voting = new VotingDaoV1() { voteBlockedUntil = DateTime.UtcNow, isBlocked = false, countAmount = 0 },
                LeagueEntries = LeagueEntryList 
            }).ConfigureAwait(false);
        }

        // This method updates the puuid for a player in the database. It was used after the api key changed to another class
        private async Task UpdatePuuidForPlayerAsync(string userName, string tag, CancellationToken stoppingToken)
        {
            // Fetch latest account and summoner data
            var accountDto = await GetAccountDto(userName, tag, stoppingToken);
            var summonerDto = await GetSummonerDto(accountDto.puuid, stoppingToken);

            // Build filter for the player
            var filter = Builders<PlayerDaoV1>.Filter.Where(u => u.UserName == userName && u.Tag == tag);

            // Build update for puuid in Account and Summoner
            var update = Builders<PlayerDaoV1>.Update
                .Set(p => p.Account.puuid, accountDto.puuid)
                .Set(p => p.Summoner.puuid, summonerDto.puuid);

            // Update the document in the collection
            await _playersCollection.UpdateOneAsync(filter, update);
        }

        private async Task<AccountDtoV1> GetAccountDto(string userName, string tag, CancellationToken stoppingToken)
        {
            try
            {
                var url = $"{riotIdByNameAndTag}{userName}/{tag}?api_key={_riotApi.GetCurrentApiKey()}";
                var response = await _httpClient.GetAsync(url, stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<RiotErrorCode>();
                    _logger.LogError($"Error during reading of account information: {errorResponse?.Status.message}");
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

        private async Task<SummonerDtoV1> GetSummonerDto(string puuid, CancellationToken stoppingToken)
        {
            try
            {

                var url = $"{riotSummonerByPuuid}{puuid}?api_key={_riotApi.GetCurrentApiKey()}";
                var response = await _httpClient.GetAsync(url, stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<RiotErrorCode>();
                    _logger.LogError($"Error during reading of summoner info with following error: {errorResponse?.Status.message}");
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

        private async Task<List<LeagueEntryDtoV1>> GetLeagueEntryListDto(string id, CancellationToken stoppingToken)
        {
            try
            {
                var url = $"{riotLeagueApi}{id}?api_key={_riotApi.GetCurrentApiKey()}";
                var response = await _httpClient.GetAsync(url, stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    return []; // Return an empty list instead of null
                }
                var content = await response.Content.ReadFromJsonAsync<LeagueEntryDtoV1[]>().ConfigureAwait(false);
                return content?.ToList() ?? []; // Handle potential null content
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of league information. Exception {e}");
                throw;
            }
        }

        private async Task CompareStandings(List<Standings> newStandings, CancellationToken stoppingToken)
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
                    await NotifyDiscord(swaps, stoppingToken).ConfigureAwait(false);
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
            var filter = Builders<StoredStandingsDaoV1>.Filter.Empty;
            var sort = Builders<StoredStandingsDaoV1>.Sort.Ascending(x => x.Id);
            var update = Builders<StoredStandingsDaoV1>.Update.Set(x => x.Standings, newStandings);

            var options = new FindOneAndUpdateOptions<StoredStandingsDaoV1>
            {
                Sort = sort
            };
            var result = await _standingsCollection.FindOneAndUpdateAsync(filter, update, options);
            if (result is null)
            {
                await _standingsCollection.InsertOneAsync(new StoredStandingsDaoV1() { Id = "standingstable", Standings = newStandings });
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

        private async Task NotifyDiscord(List<(int OriginalIndex, int NewIndex, Standings Item)> swappedPlayers, CancellationToken stoppingToken)
        {
#if DEBUG
            return;
#endif
            var returnMessage = "";
            foreach (var (OriginalIndex, NewIndex, Item) in swappedPlayers)
            {
                returnMessage += $"{Item.name} moved to new position {NewIndex + 1} \n";
            }
            var payload = new
            {
                content = $"There was a change to the gate keeper rankings.\n{returnMessage}",
                username = "The Gate Keeper"
            };

            var json = JsonSerializer.Serialize(payload);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync(_webhookUrl, contentData, stoppingToken);
        }

        private async Task NotifyDiscordGateKeeperPlaying(CancellationToken stoppingToken)
        {
#if DEBUG
            return;
#endif
            try
            {
                var emptyFilter = Builders<GateKeeperInformationDaoV1>.Filter.Empty;
                var gateKeeperInfo = await _gateKeeperCollection.Find(_ => true).FirstAsync();
                var gateKeeper = _playersCollection.Find(x => x.UserName.Equals(gateKeeperInfo.Name)).First();
                var url = $"{riotSpectatorId}{gateKeeper.Account.puuid}?api_key={_riotApi.GetCurrentApiKey()}";
                var response = await _httpClient.GetAsync(url, stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    var update = Builders<GateKeeperInformationDaoV1>.Update.Set(doc => doc.GameId, 0);
                    await _gateKeeperCollection.UpdateOneAsync(emptyFilter, update);
                    return;
                }
                var spectatorDto = await response.Content.ReadFromJsonAsync<SpectatorDtoV1>().ConfigureAwait(false);
                if (gateKeeperInfo.GameId == spectatorDto?.gameId || spectatorDto?.gameMode != "NORMAL")
                {
                    return;
                }

                var newUpdate = Builders<GateKeeperInformationDaoV1>.Update.Set(doc => doc.GameId, spectatorDto?.gameId ?? 0);
                await _gateKeeperCollection.UpdateOneAsync(emptyFilter, newUpdate);

                var payload = new
                {
                    content = $"The Gate Keeper is actual live in a game and need support by @everyone",
                    username = "The Gate Keeper"
                };

                var json = JsonSerializer.Serialize(payload);
                var contentData = new StringContent(json, Encoding.UTF8, "application/json");

                await _httpClient.PostAsync(_webhookUrl, contentData, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during sending of discord message. Exception {ex}");
                throw;
            }

        }

        private static int GetCombinedPoints(string tier, string rank, int leaguePoints)
        {
            var tierPoints = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "BRONZE", 400 },
                { "SILVER", 800 },
                { "GOLD", 1200 },
                { "PLATINUM", 1600 },
                { "EMERALD", 2000 },
                { "DIAMOND", 2400 },
                { "MASTER", 2800 },
                { "GRANDMASTER", 3200 },
                { "CHALLENGER", 3600 }
            };

            var rankPoints = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "IV", 0 },
                { "III", 100 },
                { "II", 200 },
                { "I", 300 }
            };

            int total = 0;
            if (tierPoints.TryGetValue(tier, out var tPoints))
                total += tPoints;
            if (rankPoints.TryGetValue(rank, out var rPoints))
                total += rPoints;

            total += leaguePoints;
            return total;
        }
    }
}
