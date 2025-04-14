using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Docker;

namespace AppServiceSidecars.Core.Tests.Docker;

public class DockerCommandGeneratorTests
{
    [Fact]
    public void GenerateLoginCommand_ShouldReturnCorrectCommand()
    {
        // Arrange
        var registry = "myregistry.azurecr.io";
        var username = "myusername";
        var passwordSecret = "mypassword";

        // Act
        var command = DockerCommandGenerator.GenerateLoginCommand(registry, username, passwordSecret);

        // Assert
        Assert.Equal("login myregistry.azurecr.io -u myusername -p mypassword", command);
    }

    [Fact]
    public void GenerateRunCommand_ShouldReturnCorrectCommand()
    {
        // Arrange
        var parameters = new DockerRunCommandParams
        {
            ContainerName = "mycontainer",
            SourcePort = 8080,
            TargetPort = 80,
            EnvironmentVariables = new Dictionary<string, string> { { "ENV_VAR", "value" } },
            VolumeMounts = new Dictionary<string, string> { { "/host/path", "/container/path" } },
            Image = "myimage",
            StartupCommand = "mycommand"
        };

        // Act
        var command = DockerCommandGenerator.GenerateRunCommand(parameters);

        // Assert
        Assert.Equal("run -d --label sidecar_cli=true --name mycontainer -p 8080:80 -e ENV_VAR=value -v /host/path:/container/path myimage mycommand", command);
    }

    [Fact]
    public void GenerateCommandToFetchLogs_ShouldReturnCorrectCommand()
    {
        // Arrange
        var options = new DockerLogsCommandParams
        {
            Follow = true,
            Since = "2023-01-01T00:00:00",
            Until = "2023-01-02T00:00:00",
            Tail = 100,
            Timestamps = true,
            ContainerName = "mycontainer"
        };

        // Act
        var command = DockerCommandGenerator.GenerateCommandToFetchLogs(options);

        // Assert
        Assert.Equal(" logs  --follow --since 2023-01-01T00:00:00 --until 2023-01-02T00:00:00 --tail 100 --timestamps mycontainer", command);
    }

    [Fact]
    public void GenerateListContainersCommand_ShouldReturnCorrectCommand()
    {
        // Act
        var command = DockerCommandGenerator.GenerateListContainersCommand();

        // Assert
        Assert.Equal("ps -a --filter \"label=sidecar_cli=true\" --format \"{{.Names}}\"", command);
    }
}