using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Serilog;
using Worms.Cli.Resources.Local.Schemes;
using Worms.Resources;

namespace Worms.Commands.Resources.Schemes
{
    internal class GetScheme : Command
    {
        public static readonly Argument<string> SchemeName = new("name",
            "Optional: The name or search pattern for the Scheme to be retrieved. Wildcards (*) are supported");

        public GetScheme() : base("scheme", "Retrieves information for Worms Schemes (.wsc files)")
        {
            AddAlias("schemes");
            AddAlias("wsc");
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GetSchemeHandler : ICommandHandler
    {
        private readonly ResourceGetter<LocalScheme> _schemesRetriever;
        private readonly ILogger _logger;

        public GetSchemeHandler(ResourceGetter<LocalScheme> schemesRetriever, ILogger logger)
        {
            _schemesRetriever = schemesRetriever;
            _logger = logger;
        }

        public int Invoke(InvocationContext context) =>
            Task.Run(async () => await InvokeAsync(context)).Result;

        public async Task<int> InvokeAsync(InvocationContext context)
        {
            var name = context.ParseResult.GetValueForArgument(GetScheme.SchemeName);
            var cancellationToken = context.GetCancellationToken();

            try
            {
                var windowWidth = Console.WindowWidth == 0 ? 80 : Console.WindowWidth;
                await _schemesRetriever.PrintResources(name, Console.Out, windowWidth, _logger,
                    cancellationToken);
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