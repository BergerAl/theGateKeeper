using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server.RiotsApiService
{
    public interface IRiotApi
    {
        public Task<IEnumerable<FrontEndInfoDtoV1>> GetAllRanks();
        public string GetCurrentApiKey();
        public Task<RankTimeLineEntryDaoV1> GetHistory(string userName);
        public bool SetNewApiKey(string apiKey);
    }
}
