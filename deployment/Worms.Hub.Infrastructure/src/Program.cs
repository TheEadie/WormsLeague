using System.Threading.Tasks;
using Pulumi;

namespace Worms.Hub.Infrastructure;

public static class Program
{
    public static Task<int> Main() => Deployment.RunAsync<WormsHub>();
}
