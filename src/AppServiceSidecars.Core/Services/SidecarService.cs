using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Docker;
using AppServiceSidecars.Core.Services.Logger;
using AppServiceSidecars.Core.Services.Utils;
using System.ComponentModel;

namespace AppServiceSidecars.Core.Services;

public class SidecarService : ISidecarService
{
    private readonly DockerService _dockerService;

    public SidecarService() => _dockerService = new DockerService();

    public async Task SpinUpAllContainersAsync(SidecarsConfig config)
    {
        // Tear down existing containers before running Up.
        await TearDownAllRunningContainersAsync();

        foreach (var container in config.Containers)
        {
            LoggerService.DisplayBox($"Spinning up container: {container.Name}");

            if (container.AuthType.Equals("UserCredential", StringComparison.OrdinalIgnoreCase))
            {
                LoggerService.Info($"- Container {container.Name} uses an Image that needs login");

                var registry = container.Image.Split('/')[0];
                await _dockerService.LoginToRegistryAsync(registry, container.UserName, container.PasswordSecret, CancellationToken.None);

                LoggerService.AddNewLine();
            }

            await _dockerService.RunContainerAsync(config.GetDockerRunCommandArgs(container.Name), CancellationToken.None);
        }
    }

    public async Task TearDownAllRunningContainersAsync()
    {
        var containers = await _dockerService.ListAllContainers(shouldLogToConsole: false,  CancellationToken.None);

        if (containers.Length != 0)
        {
            LoggerService.DisplayBox($"Tear down previously running containers");
            LoggerService.Info($"- Detected {containers.Length} running containers. Stopping and removing them.");
        }

        foreach (var container in containers)
        {
            await _dockerService.RemoveContainerAsync(container, CancellationToken.None);
            LoggerService.Info($"- Stopped : {container}");
        }

        LoggerService.Info("");
    }

    public async Task LogsAsync(string containerName, bool follow, string since, string until, int tail, bool timestamps)
    {
        var options = new DockerLogsCommandParams
        {
            Follow = follow,
            Since = since,
            Until = until,
            Tail = tail,
            Timestamps = timestamps
        };

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            LoggerService.Warning("Cancellation requested. Exiting log streaming...");
            cts.Cancel();
        };

        if (!string.IsNullOrWhiteSpace(containerName))
        {
            await _dockerService.GetContainerLogsAsync(containerName, options, cts.Token);
        }
        else
        {
            var containers = await _dockerService.ListAllContainers(shouldLogToConsole:false, CancellationToken.None);

            var tasks = new List<Task>();

            foreach (var container in containers)
            {
                LoggerService.Info($"--- Logs for container {container} ---");

                tasks.Add(_dockerService.GetContainerLogsAsync(container, options, cts.Token));
            }

            try { await Task.WhenAll(tasks); }
            catch (OperationCanceledException) { /* Expected on Ctrl+C */ }
        }
    }

    public async Task BuildImageAsync(string context, string dockerfile, Dictionary<string, string> buildArgs, string imageName)
    {
        LoggerService.Info($"Building image {imageName} from context {context} and Dockerfile {dockerfile}");

        await _dockerService.BuildImageAsync(new DockerBuildCommandParams
        {
            BuildContext = context,
            DockerfilePath = dockerfile,
            BuildArgs = buildArgs,
            ImageName = imageName
        }, CancellationToken.None);
    }
}