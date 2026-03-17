namespace Worms.Hub.Infrastructure.Azure;

internal static class Utils
{
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

    public static async Task<string> GetMyPublicIp()
    {
        using var http = new HttpClient();
        return (await http.GetStringAsync(new Uri("https://api.ipify.org"))).Trim();
    }
}
