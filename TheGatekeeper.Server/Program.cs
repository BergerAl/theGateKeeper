using Mcrio.Configuration.Provider.Docker.Secrets;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using TheGateKeeper.Server;
using TheGateKeeper.Server.BackgroundWorker;
using TheGateKeeper.Server.InfrastructureService;
using TheGateKeeper.Server.RiotsApiService;
using TheGateKeeper.Server.VotingService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDockerSecrets();

// Add services to the container.
builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddSingleton<IRiotApi, RiotApi>();
builder.Services.AddSingleton<IVotingService, VotingService>();
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
    var connectionString = builder.Configuration["MongoDBSettingsConnectionString"] ?? builder.Configuration["MongoDBSettings:ConnectionString"];
    var user = builder.Configuration["MongoDBSettingsUser"] ?? builder.Configuration["MongoDBSettings:User"];
    var password = builder.Configuration["MongoDBSettingsPassword"] ?? builder.Configuration["MongoDBSettings:Password"];
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
builder.Services.AddHostedService<StartUpService>();
builder.Services.AddHostedService<BackgroundWorker>();
builder.Services.AddHostedService<ScheduledTaskService>();
builder.Services.AddSignalR();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
// Configure the HTTP request pipeline.
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
app.MapHub<EventHub>("/timedOutUserVote");
app.UseAuthorization();

app.MapControllers();

app.Run();
