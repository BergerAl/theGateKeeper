using MongoDB.Driver;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;

        public RiotApi(IMongoClient mongoClient, ILogger<RiotApi> logger) {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _logger = logger;
        }

        public async Task<IEnumerable<FrontEndInfo>> GetAllRanks()
        {
            try
            {
                var players = await _collection.Find(_ => true).ToListAsync();
                return players.PlayerToFrontEndInfo().SortUsers();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of users. Exception {e}");
                return [new FrontEndInfo()];
            }
        }
    }
}
