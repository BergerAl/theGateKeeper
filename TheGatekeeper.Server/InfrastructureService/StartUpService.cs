using MongoDB.Driver;
using TheGatekeeper.Contracts;

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
                await _database.CreateCollectionAsync("ranktimeline", new CreateCollectionOptions
                {
                    Capped = false,
                    MaxSize = null,
                    MaxDocuments = null
                });
                var collection = _database.GetCollection<StoredStandingsDaoV1>("standings");
                var filter = Builders<StoredStandingsDaoV1>.Filter.Eq("_id", "standingstable");
                if (!await collection.Find(filter).AnyAsync())
                {
                    await collection.InsertOneAsync(new StoredStandingsDaoV1() { Id = "standingstable", Standings = [] });
                }
                var appConfigCollection = _database.GetCollection<AppConfigurationDaoV1>("appConfiguration");
                if (!await appConfigCollection.Find(_ => true).AnyAsync())
                {
                    await appConfigCollection.InsertOneAsync(new AppConfigurationDaoV1() { DisplayedView = DisplayedView.DefaultPage, VotingDisabled = false, DisplayResultsBar = false });
                }

                var gateKeeperCollection = _database.GetCollection<GateKeeperInformationDaoV1>("gateKeeperInfo");
                if (!await gateKeeperCollection.Find(_ => true).AnyAsync())
                {
                    await gateKeeperCollection.InsertOneAsync(new GateKeeperInformationDaoV1() { Name = "", GameId = 0 });
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
