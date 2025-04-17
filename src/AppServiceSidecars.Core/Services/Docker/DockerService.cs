using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.CommandExecutor;
using AppServiceSidecars.Core.Services.Logger;
using System.Text.RegularExpressions;

namespace AppServiceSidecars.Core.Services.Docker;

public partial class DockerService : IDockerService
{
    private readonly CommandExecutorService _commandExecutor;

    public DockerService()
    {
        _commandExecutor = new CommandExecutorService("docker");
    }

    public async Task<bool> IsDockerRunning()
    {
        try
        {
            await _commandExecutor.ExecuteCommandAsync(new CommandArgs
            {
                Arguments = "info",
                CaptureOutput = true,
                LogToConsole = false
            }, CancellationToken.None);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string[]> ListAllContainers(bool shouldLogToConsole = true, CancellationToken cancellationToken = default)
    {
        var args = new CommandArgs
        {
            Arguments = DockerCommandGenerator.GenerateListContainersCommand(),
            CaptureOutput = true,
            LogToConsole = shouldLogToConsole
        };

        var output = await _commandExecutor.ExecuteCommandAsync(args, cancellationToken);
        return output.Trim().Split("\n").Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
    }

    public async Task RunContainerAsync(DockerRunCommandParams runCommandParams, CancellationToken cancellationToken)
    {
        var args = new CommandArgs
        {
            Arguments = DockerCommandGenerator.GenerateRunCommand(runCommandParams),
            CaptureOutput = false,
            LogToConsole = false
        };

        var sanitizedArgs = SanitizeEnvironmentVariablesRegex().Replace(args.Arguments, "-e $1=****");

        LoggerService.Info($"- Running container with command:\n  docker {sanitizedArgs}");
        await _commandExecutor.ExecuteCommandAsync(args, cancellationToken);

        LoggerService.Info($"\n :check_mark_button: Container {runCommandParams.ContainerName} started successfully.\n");
    }

    public async Task<bool> RemoveContainerAsync(string containerName, CancellationToken cancellationToken)
    {
        var args = new CommandArgs
        {
            Arguments = $"rm -f {containerName}",
            CaptureOutput = true,
            LogToConsole = false
        };

        var output = await _commandExecutor.ExecuteCommandAsync(args, cancellationToken);

        return !string.IsNullOrWhiteSpace(output);
    }

    public async Task<string> GetContainerLogsAsync(string containerName, DockerLogsCommandParams commandParams, CancellationToken cancellationToken)
    {
        var args = new CommandArgs
        {
            Arguments = DockerCommandGenerator.GenerateCommandToFetchLogs(containerName, commandParams),
            CaptureOutput = false,
            LogToConsole = true,
            LogPrefix = $"container : {containerName}",
            ShouldIncludeTimestamp = false
        };

        return await _commandExecutor.ExecuteCommandAsync(args, cancellationToken);
    }

    public Task<bool> LoginToRegistryAsync(string registry, string username, string password, CancellationToken cancellationToken)
    {
        var args = new CommandArgs
        {
            Arguments = DockerCommandGenerator.GenerateLoginCommand(registry, username, password),
            LogToConsole = false,
            CaptureOutput = true
        };

        return _commandExecutor.ExecuteCommandAsync(args, cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    LoggerService.Error($"- Failed to login to Docker registry: {task.Exception?.Message}");
                    return false;
                }

                var output = task.Result;
                return output.Contains("- Login Succeeded", StringComparison.OrdinalIgnoreCase);

            }, cancellationToken);
    }

    public async Task BuildImageAsync(DockerBuildCommandParams commandParams, CancellationToken cancellationToken)
    {
        var command = DockerCommandGenerator.GetBuildCommand(commandParams);

        LoggerService.Info($"- Building image with command:\n  docker {command}");

        var commandArgs = new CommandArgs
        {
            Arguments = command,
            CaptureOutput = false,
            LogToConsole = true
        };

        await _commandExecutor.ExecuteCommandAsync(commandArgs, cancellationToken);
    }

    /// <summary>
    /// A regex pattern to match and sanitize Docker environment variable arguments.
    /// This pattern identifies arguments in the format `-e <env_var_name>=<env_var_value>`
    /// and captures the environment variable name (<env_var_name>) for replacement.
    /// </summary>
    [GeneratedRegex(@"-e\s+(\w+)=\S+")]
    private static partial Regex SanitizeEnvironmentVariablesRegex();
}
