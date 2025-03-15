namespace TheGateKeeper.Server.VotingService
{
    public interface IVotingService
    {
        public Task<VotingCallResult> VoteForUser(string userName);
    }
}
