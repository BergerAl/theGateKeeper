using Mcrio.Configuration.Provider.Docker.Secrets;
using TheGateKeeper.Server.RiotsApiService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDockerSecrets();

// Add services to the container.
builder.Services.AddSingleton<IRiotApi, RiotApi>();
builder.Services.AddHttpClient();
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

app.UseCors("_myAllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseRouting();
//app.MapHub<EventHub>("/workpieceEvent");
app.UseAuthorization();

app.MapControllers();

app.Run();
