using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace TheGateKeeper.Server.AppControl
{
    public interface IAppControl
    {
        Task<AppConfigurationDtoV1> GetConfigurationAsync();
        Task UpdateConfigurationAsync(AppConfigurationDtoV1 config);
    }

    public class AppControl : IAppControl
    {
        private readonly ILogger<AppControl> _logger;
        private readonly IMongoCollection<AppConfigurationDtoV1> _appConfiguration;
        private readonly IHubContext<EventHub> _eventHub;

        public AppControl(ILogger<AppControl> logger, IMongoClient mongoClient, IHubContext<EventHub> eventHub)
        {
            _logger = logger;
            _eventHub = eventHub;
            var database = mongoClient.GetDatabase("gateKeeper");
            _appConfiguration = database.GetCollection<AppConfigurationDtoV1>("appConfiguration");
        }

        public async Task<AppConfigurationDtoV1> GetConfigurationAsync()
        {
            var config = await _appConfiguration.Find(c => true).FirstOrDefaultAsync();
            return config ?? new AppConfigurationDtoV1();
        }

        public async Task UpdateConfigurationAsync(AppConfigurationDtoV1 appConfigurationDto)
        {
            var existingConfig = await GetConfigurationAsync();

            if (existingConfig is not null)
            {
                var emptyFilter = Builders<AppConfigurationDtoV1>.Filter.Empty;
                var update = Builders<AppConfigurationDtoV1>.Update.Set(doc => doc, appConfigurationDto);
                await _appConfiguration.UpdateOneAsync(emptyFilter, update);
                _logger.LogDebug($"Updated app configuration with Id: {existingConfig.Id}");
                try
                {
                    await _eventHub.Clients.All.SendAsync("UpdateConfigurationView", new FrontendAppConfigurationDaoV1
                    {
                        DisplayedView = appConfigurationDto.DisplayedView
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending messages to frontend on UpdateConfigurationAsync: {ex.Message}");
                }

            }
            else
            {
                await _appConfiguration.InsertOneAsync(appConfigurationDto);
            }
        }
    }
}
