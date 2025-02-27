namespace TheGateKeeper.Server.RiotsApiService
{
    public interface IRiotApi
    {
        public Task<IEnumerable<FrontEndInfo>> GetAllRanks();
    }
}
