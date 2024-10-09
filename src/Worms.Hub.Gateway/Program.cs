using Microsoft.IdentityModel.Tokens;
using Worms.Hub.Gateway;
using Worms.Hub.Gateway.API.Middleware;
using Worms.Hub.Storage;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(options => options.SingleLine = true);
builder.Configuration.AddEnvironmentVariables("WORMS_");

builder.Services.AddControllers()
    .ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new InternalControllerProvider()));
builder.Services.AddApiVersioning();
builder.Services.AddAuthentication()
    .AddJwtBearer(
        options =>
            {
                options.Authority = builder.Configuration.GetValue<string>("Auth:Authority");
                options.Audience = builder.Configuration.GetValue<string>("Auth:Audience");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = builder.Configuration.GetValue<string>("Auth:NameClaim"),
                    RoleClaimType = builder.Configuration.GetValue<string>("Auth:PermissionsClaim")
                };
            });
builder.Services.AddAuthorization();
builder.Services.AddHubStorageServices().AddGatewayServices();
builder.Services.AddOpenTelemetryWormsHub();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    _ = app.UseDeveloperExceptionPage();
    _ = app.MapControllers(); //.AllowAnonymous();
}
else
{
    _ = app.MapControllers();
}

app.UseRequestLogging();

app.Run();
