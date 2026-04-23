using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Worms.Armageddon.Gifs;

[PublicAPI]
public static class ServiceRegistration
{
    public static IServiceCollection AddWormsArmageddonGifsServices(this IServiceCollection builder) =>
        builder.AddScoped<GifCreator>();
}
