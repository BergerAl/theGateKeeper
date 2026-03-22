using MongoDB.Driver;
using TheGateKeeper.Server.InfrastructureService;

namespace TheGateKeeper.Server.WheelService
{
    public interface IWheelService
    {
        Task<List<string>> GetOptionsAsync();
        Task SaveOptionsAsync(List<string> options);
        Task NotifySpinResultAsync(string selectedUser, string result);
    }

    public class WheelService : IWheelService
    {
        private readonly IMongoCollection<WheelConfigDaoV1> _collection;
        private readonly IWebPushNotificationService _pushService;
        private readonly ILogger<WheelService> _logger;

        public WheelService(
            IMongoClient mongoClient,
            IWebPushNotificationService pushService,
            ILogger<WheelService> logger)
        {
            _pushService = pushService;
            _logger = logger;
            _collection = mongoClient.GetDatabase("gateKeeper")
                .GetCollection<WheelConfigDaoV1>("wheelConfig");
        }

        public async Task<List<string>> GetOptionsAsync()
        {
            var config = await _collection.Find(_ => true).FirstOrDefaultAsync();
            return config?.Options ?? [];
        }

        public async Task SaveOptionsAsync(List<string> options)
        {
            var existing = await _collection.Find(_ => true).FirstOrDefaultAsync();
            if (existing == null)
            {
                await _collection.InsertOneAsync(new WheelConfigDaoV1 { Options = options });
            }
            else
            {
                var update = Builders<WheelConfigDaoV1>.Update.Set(d => d.Options, options);
                await _collection.UpdateOneAsync(d => d.Id == existing.Id, update);
            }
        }

        public async Task NotifySpinResultAsync(string selectedUser, string result)
        {
            var title = "The Wheel Has Spoken!";
            var body = $"{selectedUser} spun the wheel and landed on {result}!";
            try
            {
                await _pushService.SendNotificationToAllAsync(title, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send wheel spin notification.");
            }
        }
    }
}
