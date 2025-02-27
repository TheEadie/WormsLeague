using System.Globalization;
using Syroot.Worms.Armageddon;
using Worms.Armageddon.Files.Schemes.Text;
using Worms.Cli.Logging.TableOutput;
using Worms.Cli.Resources.Local.Schemes;

namespace Worms.Cli.Resources.Schemes;

internal sealed class SchemeTextPrinter(ISchemeTextWriter schemeTextWriter) : IResourcePrinter<LocalScheme>
{
    public void Print(TextWriter writer, IReadOnlyCollection<LocalScheme> resources, int outputMaxWidth)
    {
        var tableBuilder = new TableBuilder(outputMaxWidth);
        tableBuilder.AddColumn("NAME", [.. resources.Select(x => x.Name)]);
        tableBuilder.AddColumn("CONTEXT", [.. resources.Select(x => x.Context)]);
        tableBuilder.AddColumn(
            "HEALTH",
            [.. resources.Select(x => x.Details.WormEnergy.ToString(CultureInfo.CurrentCulture))]);
        tableBuilder.AddColumn(
            "TURN-TIME",
            [.. resources.Select(x => GetTurnTime(x.Details.TurnTimeInfinite, x.Details.TurnTime))]);
        tableBuilder.AddColumn(
            "ROUND-TIME",
            [.. resources.Select(x => GetRoundTime(x.Details.RoundTimeMinutes, x.Details.RoundTimeSeconds))]);
        tableBuilder.AddColumn("WORM-SELECT", [.. resources.Select(x => x.Details.WormSelect.ToString())]);
        tableBuilder.AddColumn("WEAPONS", [.. resources.Select(x => GetWeaponSummary(x.Details.Weapons))]);

        var table = tableBuilder.Build();
        TablePrinter.Print(writer, table);
    }

    public void Print(TextWriter writer, LocalScheme resource, int outputMaxWidth) =>
        schemeTextWriter.Write(resource.Details, writer);

    private static string GetTurnTime(bool infinite, byte seconds) => infinite ? "Infinite" : seconds + " secs";

    private static string GetRoundTime(byte minutes, byte seconds) =>
        seconds > 0 ? seconds + " secs" : minutes + " mins";

    private static string GetWeaponSummary(Scheme.WeaponList weapons)
    {
        var summary = "";
        foreach (var weaponName in Enum.GetValues<Weapon>())
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
