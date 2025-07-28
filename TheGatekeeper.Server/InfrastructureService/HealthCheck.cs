using Microsoft.Extensions.Diagnostics.HealthChecks;
using TheGateKeeper.Server.RiotsApiService;

namespace TheGateKeeper.Server.InfrastructureService
{
    public class HealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;

        private readonly IRiotApi _riotApi;
        private readonly string lolStatusApi = "https://euw1.api.riotgames.com/lol/status/v4/platform-data?api_key=";

        public HealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration, IRiotApi riotApi)
        {
            _httpClient = httpClientFactory.CreateClient();
            _riotApi = riotApi;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Replace with your target URL
                var response = await _httpClient.GetAsync($"{lolStatusApi}{_riotApi.GetCurrentApiKey()}",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Unhealthy($"URL returned status code {response.StatusCode}");

                }

                return HealthCheckResult.Healthy("Health check was successful");

            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}
