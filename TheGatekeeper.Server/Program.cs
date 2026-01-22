using Mcrio.Configuration.Provider.Docker.Secrets;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;
using TheGateKeeper.Server;
using TheGateKeeper.Server.AppControl;
using TheGateKeeper.Server.BackgroundWorker;
using TheGateKeeper.Server.ConnectionManager;
using TheGateKeeper.Server.InfrastructureService;
using TheGateKeeper.Server.RiotsApiService;
using TheGateKeeper.Server.VotingService;
using TheGateKeeper.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDockerSecrets();

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false);

#if DEBUG
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
    optional: true);
#endif

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Add services to the container.
builder.Services.AddSerilog(logger);
builder.Services.AddSingleton<IRiotApi, RiotApi>();
builder.Services.AddSingleton<IVotingService, VotingService>();
builder.Services.AddSingleton<IAppControl, AppControl>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddHttpClient();
#if DEBUG
builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("https://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetIsOriginAllowed((x) => true)
                   .AllowCredentials();
        });
});
# endif
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = SecretsHelper.GetSecret(builder.Configuration, "mongoDbConnectionString");
    var user = SecretsHelper.GetSecret(builder.Configuration, "mongoDbUser");
    var password = SecretsHelper.GetSecret(builder.Configuration, "mongoDbPassword");
    var connectionUri = $"mongodb+srv://{user}:{password}@{connectionString}";
    var settings = MongoClientSettings.FromConnectionString(connectionUri);
    // Set the ServerApi field of the settings object to set the version of the Stable API on the client
    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
    // Create a new client and connect to the server
    return new MongoClient(settings);
});
builder.Services.AddHealthChecks()
    .AddCheck<HealthCheck>("Riot Api Health Check", failureStatus: HealthStatus.Unhealthy)
    .AddMongoDb();
builder.Services.Configure<HostOptions>(hostOptions =>
{
    hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});
builder.Services.AddHostedService<StartUpService>();
builder.Services.AddHostedService<BackgroundWorker>();
builder.Services.AddHostedService<ScheduledTaskService>();
builder.Services.AddSignalR();
// Configure JSON options for minimal APIs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#if DEBUG
app.UseCors("_myAllowSpecificOrigins");
# endif

app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    Predicate = _ => true,
});
app.UseHttpsRedirection();
app.UseRouting();
app.MapHub<EventHub>("/backendUpdate");
app.UseAuthorization();

// Map endpoints instead of controllers
app.MapRiotApiEndpoints();
app.MapAppConfigurationEndpoints();

app.Run();
