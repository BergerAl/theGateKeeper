using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;

namespace TheGateKeeper.Server.AppControl
{
    public interface IAppControl
    {
        Task<AppConfigurationDtoV1> GetConfigurationAsync();
        Task UpdateConfigurationAsync(AppConfigurationDtoV1 config);
        Task<GateKeeperInformationDtoV1> GetGateKeeperInformation();
    }

    public class AppControl : IAppControl
    {
        private readonly ILogger<AppControl> _logger;
        private readonly IMongoCollection<AppConfigurationDaoV1> _appConfiguration;
        private readonly IHubContext<EventHub> _eventHub;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<GateKeeperInformationDaoV1> _gateKeeperCollection;

        public AppControl(ILogger<AppControl> logger, IMongoClient mongoClient, IHubContext<EventHub> eventHub, IMapper mapper)
        {
            _logger = logger;
            _eventHub = eventHub;
            _mapper = mapper;
            var database = mongoClient.GetDatabase("gateKeeper");
            _appConfiguration = database.GetCollection<AppConfigurationDaoV1>("appConfiguration");
            _gateKeeperCollection = database.GetCollection<GateKeeperInformationDaoV1>("gateKeeperInfo");
        }

        public async Task<AppConfigurationDtoV1> GetConfigurationAsync()
        {
            var config = await _appConfiguration.Find(_ => true).FirstOrDefaultAsync();
            return _mapper.Map<AppConfigurationDtoV1>(config);
        }

        public async Task<GateKeeperInformationDtoV1> GetGateKeeperInformation()
        {
            var gateKeeperInfo = await _gateKeeperCollection.Find(_ => true).FirstAsync();
            return _mapper.Map<GateKeeperInformationDtoV1>(gateKeeperInfo);
        }

        public async Task UpdateConfigurationAsync(AppConfigurationDtoV1 appConfigurationDto)
        {
            var existingConfig = await GetConfigurationAsync();

            if (existingConfig is not null)
            {
                var emptyFilter = Builders<AppConfigurationDaoV1>.Filter.Empty;
                var update = Builders<AppConfigurationDaoV1>.Update
                    .Set(doc => doc.DisplayedView, appConfigurationDto.DisplayedView)
                    .Set(doc => doc.VotingDisabled, appConfigurationDto.VotingDisabled);
                await _appConfiguration.UpdateOneAsync(emptyFilter, update);
                _logger.LogDebug($"Updated app configuration.");
                try
                {
                    await _eventHub.Clients.All.SendAsync("UpdateConfigurationView", appConfigurationDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending messages to frontend on UpdateConfigurationAsync: {ex.Message}");
                }

            }
            else
            {
                await _appConfiguration.InsertOneAsync(_mapper.Map<AppConfigurationDaoV1>(appConfigurationDto));
            }
        }
    }
}
