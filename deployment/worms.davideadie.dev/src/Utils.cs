namespace worms.davideadie.dev;

public static class Utils
{
    public static string GetResourceName(string name)
    {
        var stack = Pulumi.Deployment.Instance.StackName;
        return stack == "prod" ? name : $"{name}-{stack}";
    }
}