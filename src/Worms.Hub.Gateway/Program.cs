using Microsoft.IdentityModel.Tokens;
using Worms.Hub.Gateway;
using Worms.Hub.Gateway.API.Middleware;
using Worms.Hub.Gateway.Worker;
using Worms.Hub.ReplayProcessor.Queue;
using Worms.Hub.Storage;

var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables("WORMS_")
    .Build();

var runGateway = configuration["HUB_DISTRIBUTED"] != "true" || configuration["HUB_GATEWAY"] == "true";
var runWorker = configuration["HUB_DISTRIBUTED"] != "true" || configuration["HUB_WORKER"] == "true";
var runAsBatchJob = configuration["BATCH"] == "true";

Console.WriteLine("Running Gateway: " + runGateway);
Console.WriteLine("Running Worker: " + runWorker);
Console.WriteLine("Running As Batch Job: " + runAsBatchJob);

var builder = WebApplication.CreateBuilder(args);
_ = builder.Logging.AddSimpleConsole(options => options.SingleLine = true);
builder.Configuration.AddConfiguration(configuration);

if (runGateway)
{
    _ = builder.Services.AddControllers()
        .ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new InternalControllerProvider()));
    _ = builder.Services.AddApiVersioning();
    _ = builder.Services.AddAuthentication()
        .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration.GetValue<string>("Auth:Authority");
                options.Audience = builder.Configuration.GetValue<string>("Auth:Audience");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = builder.Configuration.GetValue<string>("Auth:NameClaim"),
                    RoleClaimType = builder.Configuration.GetValue<string>("Auth:PermissionsClaim")
                };
            });
    _ = builder.Services.AddAuthorization();
    _ = builder.Services.AddHubStorageServices().AddGatewayServices().AddReplayToProcessQueueServices();
    builder.Services.AddOpenTelemetryWormsHub();
}

if (runWorker)
{
    _ = builder.Services.AddReplayUpdaterServices();
    _ = builder.Services.AddHostedService<CheckForMessagesService>();
}

var app = builder.Build();
if (runGateway)
{
    _ = app.UseHttpsRedirection();
    _ = app.UseRouting();
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        _ = app.UseDeveloperExceptionPage();
        _ = app.MapControllers(); //.AllowAnonymous();
    }
    else
    {
        _ = app.MapControllers();
    }

    _ = app.UseRequestLogging();
}

if (runAsBatchJob)
{
    var processor = app.Services.GetService<Processor>();
    await processor!.UpdateReplay();
    return;
}

await app.RunAsync();
