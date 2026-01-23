using MongoDB.Bson;
using MongoDB.Driver;
using TheGateKeeper.Server.AppControl;
using TheGateKeeper.Server.RiotsApiService;
using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server.VotingService
{
    public class VotingService : IVotingService
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;
        private readonly IAppControl _appControl;
        public VotingService(IMongoClient mongoClient, ILogger<RiotApi> logger, IAppControl appControl)
        {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _appControl = appControl;
            _logger = logger;
        }

        public async Task<IEnumerable<VoteStandingsDtoV1>> GetVoteStandings()
        {
            try
            {
#if !DEBUG
                var config = await _appControl.GetConfigurationAsync();
                if (!config.DisplayResultsBar)
                {
                    return [];
                }
#endif
                var allPlayers = await _collection.Find(_ => true).ToListAsync();
                var pipeline = new BsonDocument[] {
                    new BsonDocument("$project", new BsonDocument
                    {
                        { "_id", 0 },
                        { "PlayerName", "$UserName" },
                        { "Votes", "$Voting.countAmount" }
                    })
                };
                var result = await _collection.Aggregate<VoteStandingsDtoV1>(pipeline).ToListAsync();
                return result.OrderByDescending(x => x.Votes);
            }
            catch (Exception e)
            {
                _logger.LogError($"Couldn't get current standings. Exception: {e}");
                return [];
            }

        }

        public async Task<VotingCallResultDtoV1> VoteForUser(string userName)
        {
            try
            {
#if !DEBUG
                var config = await _appControl.GetConfigurationAsync();
                if (config.VotingDisabled)
                {
                    return new VotingCallResultDtoV1() { Success = false, ErrorMessage = $"Voting ist currently disabled!" };
                }
#endif
                var filter = Builders<PlayerDaoV1>.Filter.Eq(doc => doc.UserName, userName);
                var player = await _collection.Find(filter).FirstOrDefaultAsync();
                if (!player.Voting.isBlocked)
                {
                    var update = Builders<PlayerDaoV1>.Update.Set(doc => doc.Voting, new VotingDaoV1() { isBlocked = true, voteBlockedUntil = DateTime.UtcNow.AddSeconds(30), countAmount = ++ player.Voting.countAmount });
                    await _collection.UpdateOneAsync(filter, update);
                    return new VotingCallResultDtoV1() { Success = true };
                }
                return new VotingCallResultDtoV1() { Success = false, ErrorMessage = $"Voting user {userName} wasn't available, because he is still blocked" };
            }
            catch (Exception e)
            {
                _logger.LogError($"Voting user {userName} wasn't succesful with Exception {e}");
                return new VotingCallResultDtoV1() { Success = false, ErrorMessage = $"Voting user {userName} wasn't succesful" };
            }

            throw new NotImplementedException();
        }
    }
}
