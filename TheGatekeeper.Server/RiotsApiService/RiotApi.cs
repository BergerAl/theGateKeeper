using AutoMapper;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace TheGateKeeper.Server.RiotsApiService
{
    public class RiotApi : IRiotApi
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;
        private string _apiKey;
        private readonly IMapper _mapper;
        private static readonly Regex ApiKeyPattern = new Regex(
            @"^RGAPI-[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public RiotApi(IMongoClient mongoClient, ILogger<RiotApi> logger, IConfiguration configuration, IMapper mapper) {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _apiKey = configuration["api_key"] ?? File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "../secrets/api_key"));
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FrontEndInfo>> GetAllRanks()
        {
            try
            {
                var players = await _collection.Find(_ => true).ToListAsync();
                return players.PlayerToFrontEndInfo(_mapper).SortUsers();
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

        public bool SetNewApiKey(string apiKey)
        {
            if(ApiKeyPattern.IsMatch(apiKey))
            {
                _apiKey = apiKey;
                return true;
            }
            return false;
        }
    }
}
