using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using TheGateKeeper.Server.RiotsApiService;
using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class MatchWatcherService(ILogger<MatchWatcherService> logger, IMongoClient client, IHttpClientFactory httpClientFactory, IHubContext<EventHub> eventHub, IMapper mapper, IRiotApi riotApi, IConfiguration configuration) : BackgroundService
    {
        private readonly IMongoCollection<PlayerDaoV1> _playersCollection = client.GetDatabase("gateKeeper")
                           .GetCollection<PlayerDaoV1>("players");
        private readonly IMongoCollection<AppConfigurationDaoV1> _appConfiguration = client.GetDatabase("gateKeeper").GetCollection<AppConfigurationDaoV1>("appConfiguration");
        private readonly ILogger<MatchWatcherService> _logger = logger;
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly IHubContext<EventHub> _eventHub = eventHub;
        private readonly IMapper _mapper = mapper;
        private readonly IRiotApi _riotApi = riotApi;
        private readonly string _matchIdsByPuuidUrl = "https://europe.api.riotgames.com/lol/match/v5/matches/by-puuid/";
        private readonly string _matchDetailsUrl = "https://europe.api.riotgames.com/lol/match/v5/matches/";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"MatchWatcherService started process");

                    var players = await _playersCollection.Find(_ => true).ToListAsync(stoppingToken);
                    
                    foreach (var player in players)
                    {
                        try
                        {
                            var puuid = player.Account.puuid;
                            if (string.IsNullOrEmpty(puuid))
                            {
                                _logger.LogWarning($"Player {player.UserName} has no puuid, skipping");
                                continue;
                            }

                            var url = $"{_matchIdsByPuuidUrl}{puuid}/ids?start=0&count=20&api_key={_riotApi.GetCurrentApiKey()}";
                            var response = await _httpClient.GetAsync(url, stoppingToken);
                            
                            if (!response.IsSuccessStatusCode)
                            {
                                _logger.LogError($"Failed to fetch match IDs for {player.UserName}. Status: {response.StatusCode}");
                                continue;
                            }

                            var matchIds = await response.Content.ReadFromJsonAsync<string[]>(stoppingToken);
                            
                            if (matchIds != null && matchIds.Length > 0)
                            {
                                _logger.LogInformation($"Player {player.UserName} ({puuid}) has {matchIds.Length} matches");
                                
                                // Process only the first match for now
                                var matchId = matchIds[0];
                                await ProcessMatchAsync(matchId, player, stoppingToken);
                            }
                            else
                            {
                                _logger.LogInformation($"Player {player.UserName} has no matches found");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing player {player.UserName}: {ex.Message}");
                        }
                    }

                    _logger.LogInformation($"MatchWatcherService finished process successfully");
                    
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("MatchWatcherService is shutting down");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during MatchWatcherService: {ex.Message}");
                }
                
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("MatchWatcherService is shutting down");
                    break;
                }
            }
        }

        private async Task ProcessMatchAsync(string matchId, PlayerDaoV1 player, CancellationToken stoppingToken)
        {
            try
            {
                // Check if this match is already the stored last match
                if (player.StoredLastMatch?.MatchId == matchId)
                {
                    _logger.LogInformation($"Match {matchId} is already the stored last match for player {player.UserName}");
                    return;
                }

                var matchUrl = $"{_matchDetailsUrl}{matchId}?api_key={_riotApi.GetCurrentApiKey()}";
                var matchResponse = await _httpClient.GetAsync(matchUrl, stoppingToken);

                if (!matchResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch match details for {matchId}. Status: {matchResponse.StatusCode}");
                    return;
                }

                var matchData = await matchResponse.Content.ReadFromJsonAsync<MatchDetailsDtoV1>(stoppingToken);

                if (matchData?.Info?.Participants == null)
                {
                    _logger.LogWarning($"Match {matchId} has no participant data");
                    return;
                }

                // Skip games shorter than 5 minutes (300 seconds)
                if (matchData.Info.GameDuration < 300)
                {
                    _logger.LogInformation($"Match {matchId} duration ({matchData.Info.GameDuration}s) is below 5 minutes, skipping");
                    return;
                }

                // Find the participant matching the player's puuid
                var participant = matchData.Info.Participants.FirstOrDefault(p => p.Puuid == player.Account.puuid);

                if (participant == null)
                {
                    _logger.LogWarning($"Player {player.UserName} not found in match {matchId}");
                    return;
                }

                // Map to StoredMatchDaoV1
                var storedMatch = new StoredMatchDaoV1
                {
                    MatchId = matchId,
                    PlayerPuuid = player.Account.puuid,
                    StoredAt = DateTime.UtcNow,
                    GameCreation = matchData.Info.GameCreation,
                    GameDuration = matchData.Info.GameDuration,
                    GameMode = matchData.Info.GameMode,
                    GameType = matchData.Info.GameType,
                    MapId = matchData.Info.MapId,
                    QueueId = matchData.Info.QueueId,
                    ChampionId = participant.ChampionId,
                    ChampionName = participant.ChampionName,
                    Kills = participant.Kills,
                    Deaths = participant.Deaths,
                    Assists = participant.Assists,
                    Win = participant.Win,
                    GoldEarned = participant.GoldEarned,
                    TotalDamageDealtToChampions = participant.TotalDamageDealtToChampions,
                    TotalDamageTaken = participant.TotalDamageTaken,
                    ChampLevel = participant.ChampLevel,
                    Item0 = participant.Item0,
                    Item1 = participant.Item1,
                    Item2 = participant.Item2,
                    Item3 = participant.Item3,
                    Item4 = participant.Item4,
                    Item5 = participant.Item5,
                    Item6 = participant.Item6
                };

                // Update the player document with the new match
                var filter = Builders<PlayerDaoV1>.Filter.Eq(p => p.Id, player.Id);
                var update = Builders<PlayerDaoV1>.Update.Set(p => p.StoredLastMatch, storedMatch);
                await _playersCollection.UpdateOneAsync(filter, update, cancellationToken: stoppingToken);
                
                // Calculate KDA and send Discord notification if notable (<=1 or >=10)
                var kda = participant.Deaths == 0 ? participant.Kills + participant.Assists : (double)(participant.Kills + participant.Assists) / participant.Deaths;
                await SendDiscordNotificationAsync(player.UserName, participant, matchData.Info.GameMode, kda, stoppingToken);
                
                _logger.LogInformation($"Successfully stored match {matchId} for player {player.UserName} - {participant.ChampionName} ({participant.Kills}/{participant.Deaths}/{participant.Assists}) {(participant.Win ? "WIN" : "LOSS")})");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing match for player {player.UserName}: {ex.Message}");
            }
        }

        private async Task SendDiscordNotificationAsync(string playerName, MatchParticipantDtoV1 participant, string gameMode, double kda, CancellationToken stoppingToken)
        {
#if DEBUG
            return;
#endif
            try
            {
                // Only send notification for exceptional performance (high or low)
                if (kda > 1 && kda < 10)
                {
                    return; // Normal performance, no notification needed
                }

                var webhookUrl = SecretsHelper.GetSecret(configuration, "discordWebhook");
                webhookUrl = webhookUrl.Trim();

                if (string.IsNullOrEmpty(webhookUrl))
                {
                    _logger.LogWarning("Discord webhook URL not configured");
                    return;
                }

                var isPraise = kda >= 10;
                var message = isPraise
                    ? new
                    {
                        content = $"🌟 **{playerName}** absolutely dominated the game!\n" +
                                  $"**Game Mode:** {gameMode}\n" +
                                  $"**Champion:** {participant.ChampionName}\n" +
                                  $"**KDA:** {participant.Kills}/{participant.Deaths}/{participant.Assists} (KDA: {kda:F2})\n" +
                                  $"**Result:** {(participant.Win ? "WIN ✅" : "LOSS ❌")}\n" +
                                  $"What an absolute legend! 🔥"
                    }
                    : new
                    {
                        content = $"🔻 **{playerName}** had a rough game!\n" +
                                  $"**Game Mode:** {gameMode}\n" +
                                  $"**Champion:** {participant.ChampionName}\n" +
                                  $"**KDA:** {participant.Kills}/{participant.Deaths}/{participant.Assists} (KDA: {kda:F2})\n" +
                                  $"**Result:** {(participant.Win ? "WIN ✅" : "LOSS ❌")}\n" +
                                  $"What a fucking terrorist!"
                    };

                var response = await _httpClient.PostAsJsonAsync(webhookUrl, message, stoppingToken);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Discord notification sent for {playerName} ({(isPraise ? "praise" : "criticism")})");
                }
                else
                {
                    _logger.LogWarning($"Failed to send Discord notification. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending Discord notification: {ex.Message}");
            }
        }
    }
}
