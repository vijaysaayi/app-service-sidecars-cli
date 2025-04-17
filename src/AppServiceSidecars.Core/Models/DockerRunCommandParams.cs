using System.Reflection.Metadata.Ecma335;

namespace AppServiceSidecars.Core.Models;

public record DockerRunCommandParams
{
    public string ContainerName { get; init; } = string.Empty;

    public string Image { get; init; } = string.Empty;

    public int SourcePort { get; init; }

    public int TargetPort { get; init; }

    public Dictionary<string, string> EnvironmentVariables { get; init; } = [];

    public DockerRunVolumeMountParams[] VolumeMounts { get; init; } = [];

    public string StartupCommand { get; init; } = string.Empty;
}

public record DockerRunVolumeMountParams
{
    public string VolumeSubPath { get; init; } = string.Empty;

    public string ContainerMountPath { get; init; } = string.Empty;

    public bool IsReadOnly { get; init; } = false;
}
