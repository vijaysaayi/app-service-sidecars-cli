using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Logger;

namespace AppServiceSidecars.Core.Services.Utils;

public static class SidecarConfigValidator
{
    public static bool TryValidateBuildConfig(this SidecarsConfig config, string configDirectory, out SidecarConfigValidationException? exception)
    {
        var errors = new List<string>();
        exception = null;

        var numcontainersToBuild = config.Containers.Count(c => c.Build != null);

        if (numcontainersToBuild == 0)
        {
            errors.Add("No containers have a build configuration specified.");
            exception = new SidecarConfigValidationException(errors.ToArray());
            return false;
        }

        // for every container, check if the build property is set
        foreach (var container in config.Containers)
        {
            if (container.Build != null)
            {
                // check if the build context is set
                if (string.IsNullOrWhiteSpace(container.Build.Context))
                {
                    errors.Add($"Container '{container.Name}' has a build configuration but no context is set.");
                }

                var contextPath = container.Build.Context;

                if (contextPath.StartsWith('.'))
                {
                    // if the context is relative, make it absolute
                    contextPath = Path.Combine(configDirectory, contextPath);
                }

                if (!Directory.Exists(contextPath))
                {
                    errors.Add($"Container '{container.Name}' has a build configuration but the context '{contextPath}' does not exist.");
                }

                // check if the dockerfile is set
                if (string.IsNullOrWhiteSpace(container.Build.Dockerfile))
                {
                    errors.Add($"Container '{container.Name}' has a build configuration but no dockerfile is set.");
                }

                // docker file should exists in the context
                var dockerfilePath = Path.Combine(contextPath, container.Build.Dockerfile);

                if (!File.Exists(dockerfilePath))
                {
                    errors.Add($"Container '{container.Name}' has a build configuration but the dockerfile '{dockerfilePath}' does not exist.");
                }
            }
            else
            {
                LoggerService.Warning($"- Container '{container.Name}' does not have a build configuration. Skipping build.");
            }
        }

        if (errors.Count != 0)
        {
            exception = new SidecarConfigValidationException(errors.ToArray());
        }

        return true;
    }

    public static bool TryValidate(this SidecarsConfig config, out SidecarConfigValidationException? exception)
    {
        var errors = new List<string>();
        exception = null;

        // Validate that exactly one main container exists.
        var mainContainers = config.Containers.Where(c => c.IsMain).ToList();
        if (mainContainers.Count > 1)
        {
            errors.Add($"Only one main container is allowed; found {mainContainers.Count}.");
        }
        else if (mainContainers.Count == 0)
        {
            errors.Add("At least one main container must be defined.");
        }

        // Validate each container.
        foreach (var container in config.Containers)
        {
            // Check that the Image is specified.
            if (string.IsNullOrWhiteSpace(container.Image))
            {
                errors.Add($"Container '{container.Name}' must have an image specified.");
            }

            // The IsMain property is a bool; additional check for main container name.
            if (container.IsMain && string.IsNullOrWhiteSpace(container.Name))
            {
                errors.Add("Main container is missing a name.");
            }

            // Validate environment variables in each container.
            if (container.EnvironmentVariables != null && container.EnvironmentVariables.Length > 0)
            {
                // Build a set of valid app setting names.
                var validAppSettingNames = config.AppSettings.Select(a => a.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var envVar in container.EnvironmentVariables)
                {
                    if (string.IsNullOrWhiteSpace(envVar.Value))
                    {
                        errors.Add($"Environment variable '{envVar.Name}' in container '{container.Name}' has no value defined.");
                    }
                    else if (!container.IsMain && !validAppSettingNames.Contains(envVar.Value))
                    {
                        errors.Add($"In container '{container.Name}', environment variable '{envVar.Name}' value '{envVar.Value}' does not match any defined app setting name.");
                    }
                }
            }
        }
        
        if (errors.Count != 0)
        {
            exception = new SidecarConfigValidationException([.. errors]);
        }

        return errors.Count == 0;
    }
}
