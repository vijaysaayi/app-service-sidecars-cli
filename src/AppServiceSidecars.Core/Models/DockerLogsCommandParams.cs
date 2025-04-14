namespace AppServiceSidecars.Core.Models;

public record DockerLogsCommandParams
{
    public string ContainerName { get; init; } = string.Empty;

    public bool Follow { get; init; } = false;

    public string Since { get; init; } = string.Empty;

    public string Until { get; init; } = string.Empty;

    public int Tail { get; init; } = 0;

    public bool Timestamps { get; init; } = false;
}
