using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.Text.Json;
using TheGateKeeper.Server.InfrastructureService;
using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class ScheduledTaskService(ILogger<ScheduledTaskService> logger, IMongoClient client, IHubContext<EventHub> eventHub, IMapper mapper, IWebPushNotificationService pushService) : BackgroundService
    {
        private readonly IMongoCollection<PlayerDaoV1> _playersCollection = client.GetDatabase("gateKeeper")
                           .GetCollection<PlayerDaoV1>("players");
        private readonly IMongoCollection<AppConfigurationDaoV1> _appConfiguration = client.GetDatabase("gateKeeper").GetCollection<AppConfigurationDaoV1>("appConfiguration");
        private readonly ILogger<ScheduledTaskService> _logger = logger;
        private readonly IHubContext<EventHub> _eventHub = eventHub;
        private readonly IMapper _mapper = mapper;
        private readonly IWebPushNotificationService _pushService = pushService;
        private string? _appConfigJson;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"ScheduledTaskService started process");

                    var now = DateTime.UtcNow;
                    var blockedPlayers = await _playersCollection.Find(t =>
                        t.Voting.voteBlockedUntil >= now.AddSeconds(1) && t.Voting.isBlocked)
                        .ToListAsync(stoppingToken);
                    foreach (var task in blockedPlayers)
                    {
                        try
                        {
                            await _eventHub.Clients.All.SendAsync("ReceiveFrontEndInfo", blockedPlayers.PlayerToFrontEndInfo(_mapper, _logger));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error executing task {task.Id}: {ex.Message}");
                        }
                    }

                    var playersToUnblock = await _playersCollection.Find(t =>
                        t.Voting.voteBlockedUntil <= now.AddSeconds(1) && t.Voting.isBlocked)
                        .ToListAsync(stoppingToken);
                    foreach (var task in playersToUnblock)
                    {
                        try
                        {
                            await _eventHub.Clients.All.SendAsync("ReceiveFrontEndInfo", playersToUnblock.PlayerToFrontEndInfoUnblocked());
                            var filter = Builders<PlayerDaoV1>.Filter.Eq(doc => doc.UserName, task.UserName);
                            var update = Builders<PlayerDaoV1>.Update.Set(doc => doc.Voting.isBlocked, false);
                            await _playersCollection.UpdateOneAsync(filter, update);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error  executing task {task.Id}: {ex.Message}");
                        }
                    }

                    var appConfig = await _appConfiguration.Find(_ => true).FirstOrDefaultAsync();

                    // Auto-disable voting when the timer expires
                    if (appConfig?.VotingEndsAt != null && appConfig.VotingEndsAt <= now && !appConfig.VotingDisabled)
                    {
                        _logger.LogInformation("Voting timer expired, auto-disabling voting.");
                        var emptyFilter2 = Builders<AppConfigurationDaoV1>.Filter.Empty;
                        var timerUpdate = Builders<AppConfigurationDaoV1>.Update
                            .Set(doc => doc.VotingDisabled, true)
                            .Set(doc => doc.VotingEndsAt, (DateTime?)null);
                        await _appConfiguration.UpdateOneAsync(emptyFilter2, timerUpdate);
                        await _pushService.SendNotificationToAllAsync("The GateKeeper", "Voting has ended.");
                        appConfig = await _appConfiguration.Find(_ => true).FirstOrDefaultAsync();
                    }

                    if (appConfig != null)
                    {
                        var dto = _mapper.Map<AppConfigurationDtoV1>(appConfig);
                        var configJson = JsonSerializer.Serialize(dto);
                        if (configJson != _appConfigJson)
                        {
                            _appConfigJson = configJson;
                            try
                            {
                                await _eventHub.Clients.All.SendAsync("UpdateConfigurationView", dto);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error sending messages to frontend on UpdateConfigurationAsync: {ex.Message}");
                            }
                        }
                    }
                    _logger.LogInformation($"ScheduledTaskService finished process successfully");
                    
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("ScheduledTaskService is shutting down");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during ScheduledTaskService: {ex.Message}");
                }
                
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("ScheduledTaskService is shutting down");
                    break;
                }
            }
        }
    }
}
