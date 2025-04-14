using System.CommandLine;
using System.Threading.Tasks;
using System.Threading;
using AppServiceSidecars.Core.Services;

namespace AppServiceSidecarsCli.Commands;

public class DownCommand : Command<DownCommandOptions, DownCommandOptionsHandler>
{
    // Keep the hard dependency on System.CommandLine here
    public DownCommand()
        : base("down", "Tear down all containers created by the sidecar service.")
    {
        this.AddOption(new Option<bool>("--debug", "Enable verbose debug logging"));
    }
}

// Ensure DownCommandOptions implements System.CommandLine.ICommandOptions
public class DownCommandOptions : ICommandOptions
{
    public bool Debug { get; set; }
}

public class DownCommandOptionsHandler : ICommandOptionsHandler<DownCommandOptions>
{
    private readonly ISidecarService _sidecarService;

    // Inject anything here, no more hard dependency on System.CommandLine
    public DownCommandOptionsHandler(ISidecarService sidecarService)
    {
        this._sidecarService = sidecarService;
    }

    public async Task<int> HandleAsync(DownCommandOptions options, CancellationToken cancellationToken)
    {
        await _sidecarService.TearDownAllRunningContainersAsync();

        return 0;
    }
}
