namespace TheGateKeeper.Server.RiotsApiService
{
    public interface IRiotApi
    {
        public Task<IEnumerable<FrontEndInfo>> GetAllRanks();
        public string GetCurrentApiKey();
        public bool SetNewApiKey(string apiKey);
    }
}
