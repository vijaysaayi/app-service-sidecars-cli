using System.CommandLine;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Utils;
using AppServiceSidecars.Core.Services;
using System.IO;
using AppServiceSidecars.Core.Services.Logger;

namespace AppServiceSidecarsCli.Commands;

public class UpCommand : Command<UpCommandOptions, UpCommandOptionsHandler>
{
    // Keep the hard dependency on System.CommandLine here
    public UpCommand()
        : base("up", "Spin up all containers defined in the sidecars.yaml configuration file.")
    {
        this.AddOption(new Option<string>("--env", "Path to .env file"));
        this.AddOption(new Option<string>("--config-path", "Path to sidecars.yaml configuration file"));
        this.AddOption(new Option<bool>("--debug", "Enable verbose debug logging"));
    }
}

// Ensure UpCommandOptions implements System.CommandLine.ICommandOptions
public class UpCommandOptions : ICommandOptions
{
    public string Env { get; set; }

    public string ConfigPath { get; set; }

    public bool Debug { get; set; }
}

public class UpCommandOptionsHandler : ICommandOptionsHandler<UpCommandOptions>
{
    private readonly ISidecarService _sidecarService;

    // Inject anything here, no more hard dependency on System.CommandLine
    public UpCommandOptionsHandler(ISidecarService sidecarService)
    {
        this._sidecarService = sidecarService;
    }

    public async Task<int> HandleAsync(UpCommandOptions options, CancellationToken cancellationToken)
    {
        // Load environment variables from the .env file
        var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), options.Env);
        if (!File.Exists(envFilePath))
        {
            throw new FileNotFoundException($"Environment file not found: {envFilePath}", envFilePath);
        }

        var envVariables = new Dictionary<string, string>();
        foreach (var line in File.ReadLines(envFilePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            var parts = line.Split(['='], 2);
            if (parts.Length == 2)
            {
                envVariables[parts[0].Trim()] = parts[1].Trim();
            }
        }

        var config = YamlParser.ParseAndReplace<SidecarsConfig>(options.ConfigPath, envVariables);

        if (!config.TryValidate(out var validationException))
        {
            LoggerService.Error("Configuration validation failed:");
            foreach (var error in validationException.Errors)
            {
                LoggerService.Error($"- {error}");
            }

            return 1;
        }

        await _sidecarService.SpinUpAllContainersAsync(config);

        return 0;
    }
}
