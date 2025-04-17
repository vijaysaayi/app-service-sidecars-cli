namespace AppServiceSidecars.Core.Models;

public record DockerLogsCommandParams
{
    public bool Follow { get; init; } = false;

    public string Since { get; init; } = string.Empty;

    public string Until { get; init; } = string.Empty;

    public int Tail { get; init; } = 0;

    public bool Timestamps { get; init; } = false;
}

public record DockerBuildCommandParams
{
    public string ImageName { get; init; } = string.Empty;

    public string DockerfilePath { get; init; } = string.Empty;

    public string BuildContext { get; init; } = string.Empty;

    public Dictionary<string, string> BuildArgs { get; init; } = [];
}
