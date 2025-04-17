using AppServiceSidecars.Core.Models;
using Spectre.Console;

namespace AppServiceSidecars.Core.Services.Utils;

public static class SidecarsConfigExtension
{
    public static DockerBuildCommandParams GetDockerBuildCommandParams(this SidecarsConfig config, string containerName)
    {
        var container = config.Containers.FirstOrDefault(c => c.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
                         ?? throw new ArgumentException($"Container with name '{containerName}' not found in the configuration.");

        if (container.Build == null)
        {
            throw new ArgumentException($"Container '{containerName}' does not have a build configuration.");
        }

        var currentDirectory = Directory.GetCurrentDirectory();
        var contextPath = container.Build.Context;

        if (contextPath.StartsWith('.'))
        {
            // if the context is relative, make it absolute
            contextPath = Path.Combine(currentDirectory, contextPath[1..]);
        }

        return new DockerBuildCommandParams
        {
            BuildContext = contextPath,
            DockerfilePath = container.Build.Dockerfile,
            ImageName = container.Image,
            BuildArgs = container.Build.Args,
        };
    }

    public static DockerRunCommandParams GetDockerRunCommandArgs(this SidecarsConfig config, string containerName)
    {
        var container = config.Containers.FirstOrDefault(c => c.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
                        ?? throw new ArgumentException($"Container with name '{containerName}' not found in the configuration.");


        _ = int.TryParse(container.TargetPort, out var port);

        var volumeMounts = GetVolumeMounts(container, config.Name);        

        return new DockerRunCommandParams
        {
            ContainerName = container.Name,
            Image = container.Image,
            SourcePort = port,
            TargetPort = port,
            EnvironmentVariables = config.GetEnvironmentvariables(container).ToDictionary(
                envVar => envVar.Name,
                envVar => envVar.Value),
            VolumeMounts = volumeMounts,
        };
    }

    private static DockerRunVolumeMountParams[] GetVolumeMounts(Container container, string? appName)
    {
        var volumeMounts = new List<DockerRunVolumeMountParams>();

        // if volume mounts are specified, create the directories
        if (container.VolumeMounts != null && container.VolumeMounts.Length > 0)
        {
            var name = !string.IsNullOrWhiteSpace(appName) ? appName : Guid.NewGuid().ToString("N");
            var baseDirectory = Path.Combine(Path.GetTempPath(), name);
            Directory.CreateDirectory(baseDirectory);

            foreach (var volume in container.VolumeMounts)
            {
                if (!string.IsNullOrWhiteSpace(volume.VolumeSubPath))
                {
                    var volumePath = Path.Combine(baseDirectory, volume.VolumeSubPath);
                    Directory.CreateDirectory(volumePath);

                    volumeMounts.Add(new DockerRunVolumeMountParams
                    {
                        VolumeSubPath = volumePath,
                        ContainerMountPath = volume.ContainerMountPath,
                        IsReadOnly = volume.ReadOnly
                    });
                }
            }
        }

        return volumeMounts.ToArray();
    }

    private static EnvironmentVariable[] GetEnvironmentvariables(this SidecarsConfig config, Container container)
    {
        var environmentVariables = new List<EnvironmentVariable>();

        // If it is main container, pass all app settings as environment variables.
        if (container.IsMain)
        {
            foreach (var appSetting in config.AppSettings)
            {
                environmentVariables.Add(new EnvironmentVariable
                {
                    Name = appSetting.Name,
                    Value = appSetting.Value
                });
            }

            return [.. environmentVariables];
        }

        var appSettings = config.AppSettings.ToDictionary(
                appSetting => appSetting.Name,
                appSetting => appSetting.Value);

        foreach (var envVar in container.EnvironmentVariables)
        {
            environmentVariables.Add(new EnvironmentVariable
            {
                Name = envVar.Name,
                Value = appSettings[envVar.Value]
            });
        }

        return [.. environmentVariables];
    }

    public static bool TryLoadEnvironmentVariablesFromFile(string envPath, out Dictionary<string, string> envVariables)
    {
        envVariables = [];

        if (!string.IsNullOrWhiteSpace(envPath))
        {
            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), envPath);

            if (!File.Exists(envFilePath))
            {
                return false;
            }

            foreach (var line in File.ReadLines(envFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split(new[] { '=' }, 2);

                if (parts.Length == 2)
                {
                    envVariables[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }

        if (envVariables.Count > 0)
        {
            AnsiConsole.MarkupLine(":check_mark_button: Environment variables loaded from .env file.");
        }

        return true;
    }

    public static string GetMainContainerPort(this SidecarsConfig config)
    {
        var mainContainer = config.Containers.FirstOrDefault(c => c.IsMain);
        return mainContainer?.TargetPort ?? string.Empty;
    }
}
