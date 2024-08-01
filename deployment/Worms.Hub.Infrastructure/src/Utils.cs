namespace Worms.Hub.Infrastructure;

public static class Utils
{
    public static string GetResourceName(string name)
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        return stack == "prod" ? name : $"{name}-{stack}";
    }
}
