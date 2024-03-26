namespace Worms.Cli.Resources.Remote.Auth;

public interface ILoginService
{
    Task RequestLogin(CancellationToken cancellationToken);
}
