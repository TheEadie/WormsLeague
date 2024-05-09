namespace Worms.Cli.Commands.Validation;

internal sealed record ValidationRule<T>(Func<T, bool> Predicate, Func<T, string> Error);

internal sealed class RulesFor<T>
{
    private readonly List<ValidationRule<T>> _rules = [];

    public RulesFor<T> Must(Func<T, bool> rule, string message)
    {
        _rules.Add(new ValidationRule<T>(rule, _ => message));
        return this;
    }

    public RulesFor<T> Must(Func<T, bool> rule, Func<T, string> message)
    {
        _rules.Add(new ValidationRule<T>(rule, message));
        return this;
    }

    public RulesFor<T> MustNot(Func<T, bool> rule, string message)
    {
        _rules.Add(new ValidationRule<T>(x => !rule(x), _ => message));
        return this;
    }

    public RulesFor<T> MustNot(Func<T, bool> rule, Func<T, string> message)
    {
        _rules.Add(new ValidationRule<T>(x => !rule(x), message));
        return this;
    }

    public List<ValidationRule<T>> Build() => _rules;

    public static implicit operator List<ValidationRule<T>>(RulesFor<T> rules) => [.. rules.Build()];
}

internal static class Valid
{
    public static RulesFor<T> Rules<T>() => new();
}
