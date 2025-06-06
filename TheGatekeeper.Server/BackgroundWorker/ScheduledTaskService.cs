﻿using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class ScheduledTaskService(ILogger<ScheduledTaskService> logger, IMongoClient client, IHubContext<EventHub> eventHub, IMapper mapper) : BackgroundService
    {
        private readonly IMongoCollection<PlayerDaoV1> _playersCollection = client.GetDatabase("gateKeeper")
                           .GetCollection<PlayerDaoV1>("players");
        private readonly IMongoCollection<AppConfigurationDaoV1> _appConfiguration = client.GetDatabase("gateKeeper").GetCollection<AppConfigurationDaoV1>("appConfiguration");
        private readonly ILogger<ScheduledTaskService> _logger = logger;
        private readonly IHubContext<EventHub> _eventHub = eventHub;
        private readonly IMapper _mapper = mapper;
        private AppConfigurationDaoV1? _appConfig;

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
                            await _eventHub.Clients.All.SendAsync("ReceiveFrontEndInfo", blockedPlayers.PlayerToFrontEndInfo(_mapper));
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
                    if (appConfig != _appConfig)
                    {
                        _appConfig = appConfig;
                        try
                        {
                            await _eventHub.Clients.All.SendAsync("UpdateConfigurationView", _mapper.Map<AppConfigurationDtoV1>(appConfig));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error sending messages to frontend on UpdateConfigurationAsync: {ex.Message}");
                        }
                    }
                    _logger.LogInformation($"ScheduledTaskService finished process successfully");
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during ScheduledTaskService: {ex.Message}");
                }
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
