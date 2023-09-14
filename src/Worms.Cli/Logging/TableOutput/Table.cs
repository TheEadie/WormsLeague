namespace Worms.Cli.Logging.TableOutput;

internal record Table(IReadOnlyCollection<TableColumn> Columns, int RowCount);
