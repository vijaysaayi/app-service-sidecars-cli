using AppServiceSidecars.Core.Models;

namespace AppServiceSidecars.Core.Services.Utils;

public static class SidecarsConfigExtension
{
    public static DockerRunCommandParams GetDockerRunCommandArgs(this SidecarsConfig config, string containerName)
    {
        var container = config.Containers.FirstOrDefault(c => c.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
                        ?? throw new ArgumentException($"Container with name '{containerName}' not found in the configuration.");


        _ = int.TryParse(container.TargetPort, out var port);

        return new DockerRunCommandParams
        {
            ContainerName = container.Name,
            Image = container.Image,
            SourcePort = port,
            TargetPort = port,
            EnvironmentVariables = GetEnvironmentvariables(config, container).ToDictionary(
                envVar => envVar.Name,
                envVar => envVar.Value),
        };
    }

    private static EnvironmentVariable[] GetEnvironmentvariables(SidecarsConfig config, Container container)
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
}
