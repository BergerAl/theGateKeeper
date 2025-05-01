using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class ScheduledTaskService : BackgroundService
    {
        private readonly IMongoCollection<PlayerDaoV1> _playersCollection;
        private readonly ILogger<ScheduledTaskService> _logger;
        private readonly IHubContext<EventHub> _eventHub;
        private readonly IMapper _mapper;

        public ScheduledTaskService(IMongoClient client, IHubContext<EventHub> eventHub, IMapper mapper)
        {
            _mapper = mapper;
            _playersCollection = client.GetDatabase("gateKeeper")
                           .GetCollection<PlayerDaoV1>("players");
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
                        await _eventHub.Clients.All.SendAsync("ReceiveFrontEndInfo", playersToUnblock.PlayerToFrontEndInfoUnblocked(_mapper));
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
