using AutoMapper;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;
        private readonly IMongoCollection<RankTimeLineEntryDaoV1> _rankTimeLineCollection;
        private string _apiKey;
        private readonly IMapper _mapper;
        private static readonly Regex ApiKeyPattern = new Regex(
            @"^RGAPI-[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public RiotApi(IMongoClient mongoClient, ILogger<RiotApi> logger, IConfiguration configuration, IMapper mapper) {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _rankTimeLineCollection = database.GetCollection<RankTimeLineEntryDaoV1>("ranktimeline");
            _apiKey = SecretsHelper.GetSecret(configuration, "api_key");
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FrontEndInfoDtoV1>> GetAllRanks()
        {
            try
            {
                var players = await _collection.Find(_ => true).ToListAsync();
                return players.PlayerToFrontEndInfo(_mapper, _logger).SortUsers();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error during fetching of users. Exception {e}");
                return [new FrontEndInfoDtoV1()];
            }
        }

        public string GetCurrentApiKey()
        {
            return _apiKey;
        }

        public bool SetNewApiKey(string apiKey)
        {
            if(ApiKeyPattern.IsMatch(apiKey))
            {
                _apiKey = apiKey;
                return true;
            }
            return false;
        }

        public async Task<RankTimeLineEntryDaoV1> GetHistory(string userName)
        {
            try
            {
                var filter = Builders<RankTimeLineEntryDaoV1>.Filter.Eq(x => x.UserName, userName);
                var asyncCursor = await _rankTimeLineCollection.FindAsync(filter);
                return await asyncCursor.FirstAsync();
            }
            catch (Exception)
            {
                _logger.LogWarning($"No history found for user {userName}");
                return new RankTimeLineEntryDaoV1();

            }
        }
    }
}
