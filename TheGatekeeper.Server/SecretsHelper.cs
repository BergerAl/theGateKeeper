namespace TheGateKeeper.Server
{
    public static class SecretsHelper
    {
        public static Dictionary<string, string> Secrets = new Dictionary<string, string>
        {
            { "mongoDbConnectionString", "MongoDBSettings:ConnectionString" },
            { "mongoDbUser", "MongoDBSettings:User" },
            { "mongoDbPassword", "MongoDBSettings:Password" },
            { "discordWebhook", "Discord:Webhook" },
            { "api_key", "api_key" }
        };

        public static string GetSecret(IConfiguration configuration, string keyValue)
        {
            try
            {
                if (configuration is null)
                {
                    throw new ArgumentNullException(nameof(configuration));
                }
                if (keyValue is null)
                {
                    throw new ArgumentNullException(nameof(keyValue));
                }
                if (!string.IsNullOrWhiteSpace(configuration[keyValue]))
                {
                    return configuration[keyValue] ?? "";
                }
                if (!string.IsNullOrWhiteSpace(configuration[Secrets.GetValueOrDefault(keyValue)]))
                {
                    return configuration[Secrets.GetValueOrDefault(keyValue)] ?? "";
                }
                return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), $"../secrets/{keyValue}"));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
