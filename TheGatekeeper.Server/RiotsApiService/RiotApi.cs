using MongoDB.Driver;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<StoredStandingsDtoV1> _collection;

        public RiotApi(IMongoClient mongoClient, ILogger<RiotApi> logger) {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<StoredStandingsDtoV1>("standings");
            _logger = logger;
        }

        public async Task<IEnumerable<FrontEndInfo>> GetAllRanks()
        {
            try
            {
                var standingsObject = await _collection.Find(_ => true).FirstAsync();
                return standingsObject.Standings;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of users. Exception {e}");
                return [new FrontEndInfo()];
            }
        }
    }
}
