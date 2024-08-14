using System.Collections.Generic;
using System.Threading.Tasks;
using Pulumi;

namespace Worms.Hub.Infrastructure;

public static class Program
{
    public static Task<int> Main() =>
        Deployment.RunAsync(
            async () =>
                {
                    var result = await WormsHub.Create();
                    return new Dictionary<string, object?>()
                    {
                        { "database-jdbc", result.DatabaseJdbc },
                        { "database-adonet", result.DatabaseAdoNet },
                        { "database-user", result.DatabaseUser },
                        { "database-password", result.DatabasePassword },
                        { "database-version", result.DatabaseVersion },
                        { "api-url", result.ApiUrl }
                    };
                });
}
