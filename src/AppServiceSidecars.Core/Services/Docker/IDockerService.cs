using AppServiceSidecars.Core.Models;

namespace AppServiceSidecars.Core.Services.Docker
{
    public interface IDockerService
    {
        Task<string> GetContainerLogsAsync(DockerLogsCommandParams commandParams, CancellationToken cancellationToken);

        Task<bool> IsDockerRunning();

        Task<string[]> ListAllContainers(bool shouldLogToConsole = true, CancellationToken cancellationToken = default);

        Task<bool> RemoveContainerAsync(string containerName, CancellationToken cancellationToken);

        Task RunContainerAsync(DockerRunCommandParams runCommandParams, CancellationToken cancellationToken);

        Task<bool> LoginToRegistryAsync(string registry, string username, string password, CancellationToken cancellationToken);
    }
}