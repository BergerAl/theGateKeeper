using Mcrio.Configuration.Provider.Docker.Secrets;
using MongoDB.Driver;
using TheGateKeeper.Server.BackgroundWorker;
using TheGateKeeper.Server.RiotsApiService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDockerSecrets();

// Add services to the container.
builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddSingleton<IRiotApi, RiotApi>();
builder.Services.AddHttpClient();
#if DEBUG
builder.Services.AddCors(options =>
{
    options.AddPolicy("_myAllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("https://localhost:4200")
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
builder.Services.AddHostedService<BackgroundWorker>();
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
app.UseHttpsRedirection();
app.UseRouting();
//app.MapHub<EventHub>("/workpieceEvent");
app.UseAuthorization();

app.MapControllers();

app.Run();
