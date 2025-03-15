using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System.IO.Abstractions;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class ScheduledTaskService : BackgroundService
    {
        private readonly IMongoCollection<PlayerDaoV1> _playersCollection;
        private readonly IMongoCollection<StoredStandingsDtoV1> _standingsCollection;
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly IHubContext<EventHub> _eventHub;

        public ScheduledTaskService(IMongoClient client, IHubContext<EventHub> eventHub)
        {
            _playersCollection = client.GetDatabase("gateKeeper")
                           .GetCollection<PlayerDaoV1>("players");
            _standingsCollection = client.GetDatabase("gateKeeper").GetCollection<StoredStandingsDtoV1>("standings");
            _eventHub = eventHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;


                var blockedPlayers = await _playersCollection.Find(t =>
                    t.Voting.voteBlockedUntil >= now.AddSeconds(1) && t.Voting.isBlocked)
                    .ToListAsync(stoppingToken);
                foreach (var task in blockedPlayers)
                {
                    try
                    {
                        await _eventHub.Clients.All.SendAsync("ReceiveFrontEndInfo", blockedPlayers.PlayerToFrontEndInfo());
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
                        _logger.LogError($"Error executing task {task.Id}: {ex.Message}");
                    }
                }


                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
