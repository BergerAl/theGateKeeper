namespace TheGateKeeper.Server.VotingService
{
    public interface IVotingService
    {
        public Task<VotingCallResult> VoteForUser(string userName);
        public Task<IEnumerable<VoteStandingsDtoV1>> GetVoteStandings();
    }
}
