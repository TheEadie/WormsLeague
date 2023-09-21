using System.Globalization;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes.Text;
using Worms.Cli.Logging.TableOutput;
using Worms.Cli.Resources.Local.Schemes;

namespace Worms.Cli.Resources.Schemes;

internal sealed class SchemeTextPrinter : IResourcePrinter<LocalScheme>
{
    private readonly ISchemeTextWriter _schemeTextWriter;

    public SchemeTextPrinter(ISchemeTextWriter schemeTextWriter) => _schemeTextWriter = schemeTextWriter;

    public void Print(TextWriter writer, IReadOnlyCollection<LocalScheme> resources, int outputMaxWidth)
    {
        var tableBuilder = new TableBuilder(outputMaxWidth);
        tableBuilder.AddColumn("NAME", resources.Select(x => x.Name).ToList());
        tableBuilder.AddColumn("CONTEXT", resources.Select(x => x.Context).ToList());
        tableBuilder.AddColumn(
            "HEALTH",
            resources.Select(x => x.Details.WormEnergy.ToString(CultureInfo.CurrentCulture)).ToList());
        tableBuilder.AddColumn(
            "TURN-TIME",
            resources.Select(x => GetTurnTime(x.Details.TurnTimeInfinite, x.Details.TurnTime)).ToList());
        tableBuilder.AddColumn(
            "ROUND-TIME",
            resources.Select(x => GetRoundTime(x.Details.RoundTimeMinutes, x.Details.RoundTimeSeconds)).ToList());
        tableBuilder.AddColumn("WORM-SELECT", resources.Select(x => x.Details.WormSelect.ToString()).ToList());
        tableBuilder.AddColumn("WEAPONS", resources.Select(x => GetWeaponSummary(x.Details.Weapons)).ToList());

        var table = tableBuilder.Build();
        TablePrinter.Print(writer, table);
    }

    public void Print(TextWriter writer, LocalScheme resource, int outputMaxWidth) =>
        _schemeTextWriter.Write(resource.Details, writer);

    private static string GetTurnTime(bool infinite, byte seconds) => infinite ? "Infinite" : seconds + " secs";

    private static string GetRoundTime(byte minutes, byte seconds) =>
        seconds > 0 ? seconds + " secs" : minutes + " mins";

    private static string GetWeaponSummary(Scheme.WeaponList weapons)
    {
        var summary = "";
        foreach (var weaponName in (Weapon[]) Enum.GetValues(typeof(Weapon)))
        {
            var weapon = weapons[weaponName];
            if (weapon.Ammo > 0)
            {
                summary += $"{weaponName.ToString()[..2]}({weapon.Ammo}), ";
            }
        }

        if (summary.Length > 0)
        {
            summary = summary[..^2];
        }

        return summary;
    }
}
