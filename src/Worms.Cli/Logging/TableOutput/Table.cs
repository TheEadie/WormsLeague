namespace Worms.Cli.Logging.TableOutput;

internal sealed record Table(IReadOnlyCollection<TableColumn> Columns, int RowCount);
