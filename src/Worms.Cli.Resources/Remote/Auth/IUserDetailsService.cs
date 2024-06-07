namespace Worms.Cli.Resources.Remote.Auth;

public interface IUserDetailsService
{
    bool IsUserLoggedIn();

    string GetAnonymisedUserId();
}
