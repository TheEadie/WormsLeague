using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Worms.Gateway.Announcers;
using Worms.Gateway.Announcers.Slack;
using Worms.Gateway.Database;
using Worms.Gateway.Domain;
using Worms.Gateway.Dtos;
using Worms.Gateway.Validators;

namespace Worms.Gateway;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.AddSimpleConsole(options => { options.SingleLine = true; });
        builder.Configuration.AddEnvironmentVariables("WORMS_");
        builder.Services.AddControllers();
        builder.Services.AddApiVersioning();
        builder.Services.AddAuthentication()
            .AddJwtBearer(
                options =>
                    {
                        options.Authority = "https://eadie.eu.auth0.com/";
                        options.Audience = "worms.davideadie.dev";
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType = ClaimTypes.NameIdentifier,
                            RoleClaimType = "permissions"
                        };
                    });
        builder.Services.AddAuthorization();
        RegisterServices(builder);

        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void RegisterServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IRepository<GameDto>, GamesRepository>();
        builder.Services.AddSingleton<IRepository<Replay>, ReplaysRepository>();
        builder.Services.AddSingleton<ISlackAnnouncer, SlackAnnouncer>();
        builder.Services.AddSingleton<ReplayFileValidator>();
    }
}
