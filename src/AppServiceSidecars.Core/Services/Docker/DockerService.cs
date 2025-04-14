using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.CommandExecutor;
using AppServiceSidecars.Core.Services.Logger;

namespace AppServiceSidecars.Core.Services.Docker;

public class DockerService : IDockerService
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
                CaptureOutput = true
            }, CancellationToken.None);

            return true;
        }
        catch (Exception ex)
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

        LoggerService.Info($"Running container with command: {args.Arguments}");
        await _commandExecutor.ExecuteCommandAsync(args, cancellationToken);

        LoggerService.Info($"Container {runCommandParams.ContainerName} started successfully.\n");
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

    public async Task<string> GetContainerLogsAsync(DockerLogsCommandParams commandParams, CancellationToken cancellationToken)
    {
        var args = new CommandArgs
        {
            Arguments = DockerCommandGenerator.GenerateCommandToFetchLogs(commandParams),
            CaptureOutput = true
        };

        return await _commandExecutor.ExecuteCommandAsync(args, cancellationToken);
    }

    public Task<bool> LoginToRegistryAsync(string registry, string username, string password, CancellationToken cancellationToken)
    {
        var args = new CommandArgs
        {
            Arguments = DockerCommandGenerator.GenerateLoginCommand(registry, username, password),
            CaptureOutput = true
        };

        return _commandExecutor.ExecuteCommandAsync(args, cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    LoggerService.Error($"Failed to login to Docker registry: {task.Exception?.Message}");
                    return false;
                }

                var output = task.Result;
                return output.Contains("Login Succeeded", StringComparison.OrdinalIgnoreCase);

            }, cancellationToken);
    }
}
