using AppServiceSidecars.Core.Models;

namespace AppServiceSidecars.Core.Services.Utils;

public static class SidecarConfigValidator
{
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
