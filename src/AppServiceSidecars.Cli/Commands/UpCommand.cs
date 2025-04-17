using System.CommandLine;
using System.Threading.Tasks;
using System.Threading;
using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Utils;
using AppServiceSidecars.Core.Services;
using System.IO;
using AppServiceSidecars.Core.Services.Logger;
using Spectre.Console;

namespace AppServiceSidecarsCli.Commands;

public class UpCommand : Command<UpCommandOptions, UpCommandOptionsHandler>
{
    // Keep the hard dependency on System.CommandLine here
    public UpCommand()
        : base("up", "Spin up all containers defined in the sidecars.yaml configuration file.")
    {
        var configPathArgument = new Argument<string>("config-path", "Path to sidecars.yaml configuration file")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        var envOption = new Option<string>("--env", "Path to .env file");

        this.AddArgument(configPathArgument);
        this.AddOption(envOption);
        this.AddOption(new Option<bool>("--debug", "Enable verbose debug logging"));

        this.AddValidator(validator =>
        {
            var configPath = validator.FindResultFor(configPathArgument);

            this.AddValidator(validator =>
            {
                var configPath = validator.FindResultFor(configPathArgument);

                if (configPath is null || configPath.Tokens.Count == 0 || !File.Exists(configPath.Tokens[0].Value))
                {
                    validator.ErrorMessage = configPath is null || configPath.Tokens.Count == 0
                        ? "Configuration file path is missing."
                        : $"Configuration file not found: {configPath.Tokens[0].Value}";

                    return;
                }

                var envPath = validator.FindResultFor(envOption);

                if (envPath is not null && (envPath.Tokens.Count == 0 || !File.Exists(envPath.Tokens[0].Value)))
                {
                    validator.ErrorMessage = envPath.Tokens.Count == 0
                        ? "Environment file path is missing."
                        : $"Environment file not found: {envPath.Tokens[0].Value}";
                }
            });

            var envPath = validator.FindResultFor(envOption);

            if (envPath is not null && !File.Exists(envPath.Tokens[0].Value))
            {
                validator.ErrorMessage = $"Environment file not found: {envPath.Tokens[0].Value}";
            }
        });
    }
}

// Ensure UpCommandOptions implements System.CommandLine.ICommandOptions
public class UpCommandOptions : ICommandOptions
{
    public string Env { get; set; }

    public string ConfigPath { get; set; }

    public bool Debug { get; set; }
}

public class UpCommandOptionsHandler(ISidecarService sidecarService) : ICommandOptionsHandler<UpCommandOptions>
{
    private readonly ISidecarService _sidecarService = sidecarService;

    public async Task<int> HandleAsync(UpCommandOptions options, CancellationToken cancellationToken)
    {
        SidecarsConfigExtension.TryLoadEnvironmentVariablesFromFile(options.Env, out var envVariables);

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

        AnsiConsole.MarkupLine(":check_mark_button: Successfully validated configuration. \n");

        await _sidecarService.SpinUpAllContainersAsync(config);

        var mainContainerPort = config.GetMainContainerPort();

        LoggerService.AddNewLine();
        LoggerService.DisplayBox("Next Steps");
        LoggerService.Info("Use the following url to browse to your application:");
        LoggerService.Info($"- http://localhost:{mainContainerPort}");
        LoggerService.AddNewLine();
        LoggerService.Info("Use the following command to view logs:");
        LoggerService.Info($"- appservice-sidecars logs <container-name>");
        LoggerService.AddNewLine();
        LoggerService.Info("Use the following command to stop all containers:");
        LoggerService.Info($"- appservice-sidecars down");
        LoggerService.AddNewLine();

        return 0;
    }
}
