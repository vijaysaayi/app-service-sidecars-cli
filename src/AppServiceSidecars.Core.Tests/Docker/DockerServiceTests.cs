using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Docker;

namespace AppServiceSidecars.Core.Tests.Docker;

public class DockerServiceTests : IDisposable
{
    private readonly DockerService _dockerService;

    public DockerServiceTests()
    {
        _dockerService = new DockerService();
    }

    [Fact]
    public async Task IsDockerRunning_ShouldReturnTrue_WhenDockerIsRunning()
    {
        // Act
        var result = await _dockerService.IsDockerRunning();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RunContainerAsync_ShouldStartContainerSuccessfully()
    {
        // Arrange
        var containerName = "test-container";
        var image = "nginx:latest";
        var runCommandParams = new DockerRunCommandParams
        {
            ContainerName = containerName,
            Image = image
        };

        // Act
        await _dockerService.RunContainerAsync(runCommandParams, CancellationToken.None);

        // Assert
        var containers = await _dockerService.ListAllContainers();
        Assert.Contains(containerName, containers);

        // Cleanup
        await _dockerService.RemoveContainerAsync(containerName, CancellationToken.None);
    }

    [Fact]
    public async Task GetContainerLogsAsync_ShouldReturnLogs()
    {
        // Arrange
        var containerName = "test-container-1";
        var image = "nginx:latest";
        var runCommandParams = new DockerRunCommandParams
        {
            ContainerName = containerName,
            Image = image
        };

        await _dockerService.RunContainerAsync(runCommandParams, CancellationToken.None);

        var logsCommandParams = new DockerLogsCommandParams
        {
            ContainerName = containerName
        };

        // Act
        var logs = await _dockerService.GetContainerLogsAsync(logsCommandParams, CancellationToken.None);

        // Assert
        Assert.NotNull(logs);

        // Cleanup
        await _dockerService.RemoveContainerAsync(containerName, CancellationToken.None);
    }

    [Fact]
    public async Task RemoveContainerAsync_ShouldRemoveContainerSuccessfully()
    {
        // Arrange
        var containerName = "test-container-2";
        var image = "nginx:latest";
        var runCommandParams = new DockerRunCommandParams
        {
            ContainerName = containerName,
            Image = image
        };

        await _dockerService.RunContainerAsync(runCommandParams, CancellationToken.None);

        // Act
        await _dockerService.RemoveContainerAsync(containerName, CancellationToken.None);

        // Assert
        var containers = await _dockerService.ListAllContainers();
        Assert.DoesNotContain(containerName, containers);
    }

    public void Dispose()
    {
        // Cleanup any lingering containers
        _dockerService.ListAllContainers().ContinueWith(async task =>
        {
            foreach (var container in task.Result)
            {
                await _dockerService.RemoveContainerAsync(container, CancellationToken.None);
            }
        }).Wait();
    }
}