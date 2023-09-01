using System.Threading.Tasks;
using Pulumi;

namespace worms.davideadie.dev;

public static class Program
{
    public static Task<int> Main() { return Deployment.RunAsync<WormsHub>(); }
}