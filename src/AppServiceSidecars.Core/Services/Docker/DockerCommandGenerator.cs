using AppServiceSidecars.Core.Models;
using System.Text;

namespace AppServiceSidecars.Core.Services.Docker;

public static class DockerCommandGenerator
{
    private const string LabelFilter = "sidecar_cli=true";

    public static string GenerateLoginCommand(string registry, string username, string passwordSecret)
    {
        return $"login {registry} -u {username} -p {passwordSecret}";
    }

    public static string GenerateRunCommand(DockerRunCommandParams parameters)
    {
        var command = new StringBuilder($"run -d --label {LabelFilter} --name {parameters.ContainerName}");
        command.Append($" -p {parameters.SourcePort}:{parameters.TargetPort}");

        foreach (var envVar in parameters.EnvironmentVariables)
        {
            command.Append($" -e {envVar.Key}={envVar.Value}");
        }

        foreach (var volume in parameters.VolumeMounts)
        {
            command.Append($" -v {volume.Key}:{volume.Value}");
        }

        command.Append($" {parameters.Image}");
        command.Append($" {parameters.StartupCommand}");

        return command.ToString();
    }

    public static string GenerateCommandToFetchLogs(DockerLogsCommandParams options)
    {
        var command = new StringBuilder(" logs ");
        if (options.Follow) 
        {
            command.Append(" --follow"); 
        }

        if (!string.IsNullOrWhiteSpace(options.Since)) 
        {
            command.Append($" --since {options.Since}"); 
        }

        if (!string.IsNullOrWhiteSpace(options.Until)) 
        { 
            command.Append($" --until {options.Until}"); 
        }

        if (options.Tail > 0) 
        { 
            command.Append($" --tail {options.Tail}"); 
        }

        if (options.Timestamps) 
        { 
            command.Append(" --timestamps"); 
        }

        command.Append($" {options.ContainerName}");

        return command.ToString();
    }

    public static string GenerateListContainersCommand()
    {
        return $"ps -a --filter \"label={LabelFilter}\" --format \"{{{{.Names}}}}\"";
    }
}
