using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Logger;
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
            var readOnlyFlag = volume.IsReadOnly ? ":ro" : string.Empty;
            command.Append($" -v {volume.VolumeSubPath}:{volume.ContainerMountPath}:{readOnlyFlag}");
        }

        command.Append($" {parameters.Image}");
        command.Append($" {parameters.StartupCommand}");

        return command.ToString();
    }

    public static string GenerateCommandToFetchLogs(string containerName, DockerLogsCommandParams options)
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

        command.Append($" {containerName}");

        LoggerService.Info($"Fetching logs for container: {containerName} , command : {command}");

        return command.ToString();
    }

    public static string GenerateListContainersCommand()
    {
        return $"ps -a --filter \"label={LabelFilter}\" --format \"{{{{.Names}}}}\"";
    }

    public static string GetBuildCommand(DockerBuildCommandParams commandParams)
    {
        // Resolve the context path to an absolute path
        var resolvedContext = Path.GetFullPath(commandParams.BuildContext, Directory.GetCurrentDirectory());

        if (!Directory.Exists(resolvedContext))
        {
            throw new DirectoryNotFoundException($"The context directory '{resolvedContext}' does not exist.");
        }

        var args = new List<string>
        {
            "build",
            $"--file \"{commandParams.DockerfilePath}\"",
            $"--tag {commandParams.ImageName}",
            $"\"{resolvedContext}\""
        };

        foreach (var arg in commandParams.BuildArgs)
        {
            args.Add($"--build-arg {arg.Key}={arg.Value}");
        }

        return string.Join(" ", args);
    }
}
