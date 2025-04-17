using System.CommandLine;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Utils;
using AppServiceSidecars.Core.Services;
using System.IO;

namespace AppServiceSidecarsCli.Commands;

public class LogsCommand : Command<LogsCommandOptions, LogsCommandOptionsHandler>
{
    // Keep the hard dependency on System.CommandLine here
    public LogsCommand()
        : base("logs", "Fetch logs for the specified container or all containers if no name is provided.")
    {
        this.AddArgument(new Argument<string>("containerName", () => string.Empty, "Container name to fetch logs for (leave blank to fetch logs for all containers)"));

        this.AddOption(new Option<bool>("--follow", "Follow logs"));
        this.AddOption(new Option<string>("--since", "Show logs since timestamp"));
        this.AddOption(new Option<string>("--until", "Show logs until a timestamp"));
        this.AddOption(new Option<int>("--tail", () => 0, "Number of lines from the end of logs (0 means all)"));
        this.AddOption(new Option<bool>("--timestamps", "Show timestamps"));
        this.AddOption(new Option<bool>("--debug", "Enable verbose debug logging"));
    }
}

// Ensure LogsCommandOptions implements System.CommandLine.ICommandOptions
public class LogsCommandOptions : ICommandOptions
{
    public string ContainerName { get; set; } = string.Empty;

    public bool Follow { get; set; } = false;

    public string Since { get; set; } = string.Empty;

    public string Until { get; set; } = string.Empty;

    public int Tail { get; set; } = 0;

    public bool Timestamps { get; set; } = false;

    public bool Debug { get; set; }
}

public class LogsCommandOptionsHandler : ICommandOptionsHandler<LogsCommandOptions>
{
    private readonly ISidecarService _sidecarService;

    // Inject anything here, no more hard dependency on System.CommandLine
    public LogsCommandOptionsHandler(ISidecarService sidecarService)
    {
        this._sidecarService = sidecarService;
    }

    public async Task<int> HandleAsync(LogsCommandOptions options, CancellationToken cancellationToken)
    {
        await _sidecarService.LogsAsync(options.ContainerName, options.Follow, options.Since, options.Until, options.Tail, options.Timestamps);

        return 0;
    }
}
