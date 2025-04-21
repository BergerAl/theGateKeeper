using MongoDB.Driver;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;
        private string _apiKey;

        public RiotApi(IMongoClient mongoClient, ILogger<RiotApi> logger, IConfiguration configuration) {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _apiKey = configuration["api_key"] ?? File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../secrets/api_key"));
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

        public string GetCurrentApiKey()
        {
            return _apiKey;
        }

        public void SetNewApiKey(string apiKey)
        {
            _apiKey = apiKey;
        }
    }
}
