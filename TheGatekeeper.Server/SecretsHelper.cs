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
            { "api_key", "api_key" },
            { "vapid_public_key", "Vapid:PublicKey" },
            { "vapid_private_key", "Vapid:PrivateKey" },
            { "vapid_subject", "Vapid:Subject" },
            { "keycloak_admin", "Keycloak:AdminUser" },
            { "keycloak_admin_password", "Keycloak:AdminPassword" }
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
