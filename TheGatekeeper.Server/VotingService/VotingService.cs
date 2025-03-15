
using MongoDB.Driver;
using TheGateKeeper.Server.RiotsApiService;

namespace TheGateKeeper.Server.VotingService
{
    public class VotingService : IVotingService
    {
        private readonly ILogger _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;
        public VotingService(IMongoClient mongoClient, ILogger<RiotApi> logger)
        {
            var database = mongoClient.GetDatabase("gateKeeper");
            _collection = database.GetCollection<PlayerDaoV1>("players");
            _logger = logger;
        }
        public async Task<VotingCallResult> VoteForUser(string userName)
        {
            try
            {
                var filter = Builders<PlayerDaoV1>.Filter.Eq(doc => doc.UserName, userName);
                var player = await _collection.Find(filter).FirstOrDefaultAsync();
                if (!player.Voting.isBlocked)
                {
                    var update = Builders<PlayerDaoV1>.Update.Set(doc => doc.Voting, new Voting() { isBlocked = true, voteBlockedUntil = DateTime.UtcNow.AddSeconds(30) });
                    await _collection.UpdateOneAsync(filter, update);
                    return new VotingCallResult() { Success = true };
                }
                return new VotingCallResult() { Success = false, ErrorMessage = $"Voting user {userName} wasn't available, because he is still blocked" };
            }
            catch (Exception e)
            {
                _logger.LogError($"Voting user {userName} wasn't succesful with Exception {e}");
                return new VotingCallResult() { Success = false, ErrorMessage = $"Voting user {userName} wasn't succesful" };
            }

            throw new NotImplementedException();
        }
    }
}
