using System.CommandLine;
using System.Threading.Tasks;
using System.Threading;
using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Utils;
using System.IO;
using AppServiceSidecars.Core.Services.Logger;
using AppServiceSidecars.Core.Services.Docker;
using Spectre.Console;

namespace AppServiceSidecarsCli.Commands;

public class BuildCommand : Command<BuildCommandOptions, BuildCommandOptionsHandler>
{
    public BuildCommand()
        : base("build", "Build Docker images for the sidecars defined in the sidecars.yaml configuration file.")
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
    }
}

public class BuildCommandOptions : ICommandOptions
{
    public string Env { get; set; }

    public string ConfigPath { get; set; }

    public bool Debug { get; set; }
}

public class BuildCommandOptionsHandler(IDockerService dockerService) : ICommandOptionsHandler<BuildCommandOptions>
{
    private readonly IDockerService _dockerService = dockerService;

    public async Task<int> HandleAsync(BuildCommandOptions options, CancellationToken cancellationToken)
    {
        SidecarsConfigExtension.TryLoadEnvironmentVariablesFromFile(options.Env, out var envVariables);

        var config = YamlParser.ParseAndReplace<SidecarsConfig>(options.ConfigPath, envVariables);
        var directoryOfConfigFile = Path.GetDirectoryName(options.ConfigPath);

        if (!config.TryValidateBuildConfig(directoryOfConfigFile, out var validationException))
        {
            LoggerService.Error("Configuration validation failed:");
            foreach (var error in validationException.Errors)
            {
                LoggerService.Error($"- {error}");
            }

            return 1;
        }

        AnsiConsole.MarkupLine(":check_mark_button: Successfully validated configuration. \n");

        foreach (var container in config.Containers)
        {
            LoggerService.DisplayBox($"Building image for container: {container.Name}");

            var command = config.GetDockerBuildCommandParams(container.Name);

            await _dockerService.BuildImageAsync(command, cancellationToken);

            LoggerService.Info($"\n:check_mark_button: Successfully built image for container: {container.Name} \n");
        }

        return 0;
    }
}