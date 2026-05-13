using System.Data;
using System.Globalization;
using Dapper;
using JetBrains.Annotations;
using Worms.Hub.Storage.Domain;

namespace Worms.Hub.Storage.Database;

#pragma warning disable CA1819 // Dapper requires the concrete array type for constructor-based mapping
[PublicAPI]
public record ReplayDb(
    int Id,
    string Name,
    string Status,
    string Filename,
    string? FullLog,
    string? LeagueId,
    DateTime? Date,
    string? Winner,
    string[]? Teams)
{
    public Replay ToDomain() =>
        new(
            Id.ToString(CultureInfo.InvariantCulture),
            Name,
            Status,
            Filename,
            FullLog,
            LeagueId,
            Date,
            Winner,
            Teams);
}
#pragma warning restore CA1819

internal sealed class StringArrayHandler : SqlMapper.TypeHandler<string[]>
{
    public static readonly StringArrayHandler Instance = new();

    public override void SetValue(IDbDataParameter parameter, string[]? value) =>
        parameter.Value = value ?? (object)DBNull.Value;

    public override string[] Parse(object value) =>
        value as string[] ?? [];
}
