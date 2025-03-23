using MongoDB.Driver;

namespace TheGateKeeper.Server.InfrastructureService
{
    public class StartUpService : IHostedService
    {
        private readonly ILogger<StartUpService> _logger;
        private readonly IMongoDatabase _database;

        public StartUpService(IMongoClient mongoClient, ILogger<StartUpService> logger)
        {
            _database = mongoClient.GetDatabase("gateKeeper");
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting startup task...");
            try
            {
                await CreateDatabaseCollections();
                _logger.LogInformation("Startup task completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Startup task failed");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task CreateDatabaseCollections()
        {
            try
            {
                await _database.CreateCollectionAsync("players", new CreateCollectionOptions
                {
                    Capped = false,
                    MaxSize = null,
                    MaxDocuments = null
                });
                await _database.CreateCollectionAsync("standings", new CreateCollectionOptions
                {
                    Capped = false,
                    MaxSize = null,
                    MaxDocuments = null
                });
                var collection = _database.GetCollection<StoredStandingsDtoV1>("standings");
                var filter = Builders<StoredStandingsDtoV1>.Filter.Eq("_id", "standingstable");
                if (!await collection.Find(filter).AnyAsync())
                {
                    await collection.InsertOneAsync(new StoredStandingsDtoV1() { Id = "standingstable", Standings = [] });
                }
            }
            catch (MongoCommandException ex)
            {
                _logger.LogError($"MongoCommandException: {ex}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fatal exception: {ex}");
            }
        }
    }
}
