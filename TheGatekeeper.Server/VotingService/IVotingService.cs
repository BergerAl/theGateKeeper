using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server.VotingService
{
    public interface IVotingService
    {
        public Task<VotingCallResultDtoV1> VoteForUser(string userName);
        public Task<IEnumerable<VoteStandingsDtoV1>> GetVoteStandings();
    }
}
