namespace Worms.Hub.Infrastructure.Azure;

internal static class Utils
{
    private static readonly HttpClient HttpClient = new();

    public static string GetResourceName(string name)
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        return stack == "prod" ? name : $"{name}-{stack}";
    }

    public static string GetResourceNameAlphaNumericOnly(string name)
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        return stack == "prod" ? name : name + stack;
    }

    public static async Task<string> GetMyPublicIp() =>
        (await HttpClient.GetStringAsync(new Uri("https://api.ipify.org"))).Trim();
}
