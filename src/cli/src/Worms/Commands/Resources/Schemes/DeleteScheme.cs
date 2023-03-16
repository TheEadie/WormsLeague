using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Resources;

namespace Worms.Commands.Resources.Schemes
{
    internal class DeleteScheme : Command
    {
        public static readonly Argument<string> SchemeName = new("name",
            "The name of the Scheme to be deleted");

        public DeleteScheme() : base("scheme", "Delete Worms Schemes (.wsc files)")
        {
            AddAlias("schemes");
            AddAlias("wsc");
            AddArgument(SchemeName);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DeleteSchemeHandler : ICommandHandler
    {
        private readonly ResourceDeleter<LocalScheme> _resourceDeleter;
        private readonly ILogger _logger;

        public DeleteSchemeHandler(ResourceDeleter<LocalScheme> resourceDeleter, ILogger logger)
        {
            _resourceDeleter = resourceDeleter;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var name = context.ParseResult.GetValueForArgument(DeleteScheme.SchemeName);
            var cancellationToken = context.GetCancellationToken();

            try
            {
                await _resourceDeleter.Delete(name, _logger, cancellationToken);
            }
            catch (ConfigurationException exception)
            {
                _logger.Error(exception.Message);
                return 1;
            }

            return 0;
        }
    }
}