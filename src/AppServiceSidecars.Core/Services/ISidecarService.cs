using AppServiceSidecars.Core.Models;

namespace AppServiceSidecars.Core.Services
{
    public interface ISidecarService
    {
        Task LogsAsync(string containerName, bool follow, string since, string until, int tail, bool timestamps);
        Task SpinUpAllContainersAsync(SidecarsConfig config);
        Task TearDownAllRunningContainersAsync();
    }
}