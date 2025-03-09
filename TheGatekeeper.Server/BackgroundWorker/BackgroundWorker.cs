using MongoDB.Driver;

namespace TheGateKeeper.Server.BackgroundWorker
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly ILogger<BackgroundWorker> _logger;
        private readonly IMongoCollection<PlayerDaoV1> _collection;
        private readonly HttpClient _httpClient;
        public BackgroundWorker(ILogger<BackgroundWorker> logger, MongoClient mongoClient, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            var database = mongoClient.GetDatabase(configuration["MongoDBSettings:DatabaseName"]);
            _collection = database.GetCollection<PlayerDaoV1>("CollectedData");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Fetching data from API...");

                    var response = await _httpClient.GetAsync(_apiUrl, stoppingToken);
                    response.EnsureSuccessStatusCode();

                    var responseData = await response.Content.ReadAsStringAsync();

                    var dataToSave = new CollectedData
                    {
                        Timestamp = DateTime.UtcNow,
                        Data = responseData
                    };

                    await _collection.InsertOneAsync(dataToSave, cancellationToken: stoppingToken);
                    _logger.LogInformation("Data saved successfully");

                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Data fetch operation cancelled");
                    throw;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error fetching data from API");
                }
                catch (MongoException ex)
                {
                    _logger.LogError(ex, "Error saving data to MongoDB");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error occurred");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
