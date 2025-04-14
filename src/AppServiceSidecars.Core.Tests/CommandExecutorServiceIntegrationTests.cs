using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.CommandExecutor;

namespace AppServiceSidecars.Core.Tests;

public class CommandExecutorServiceIntegrationTests : IDisposable
{
    private readonly string _tempFilePath;

    public CommandExecutorServiceIntegrationTests()
    {
        // Create a temp file for testing file operations
        _tempFilePath = Path.GetTempFileName();
    }

    [Fact]
    public async Task ExecuteCommandAsync_SuccessfulCommand_ReturnsOutput()
    {
        // Arrange
        var commandExecutor = new CommandExecutorService("cmd.exe");
        var args = new CommandArgs
        {
            Arguments = GetCommandArgs("echo Hello World"),
            CaptureOutput = true
        };

        // Act
        var result = await commandExecutor.ExecuteCommandAsync(args, CancellationToken.None);

        // Assert
        Assert.Contains("Hello World", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithTimestamp_IncludesTimestampInOutput()
    {
        // Arrange
        var commandExecutor = new CommandExecutorService(GetCommand());
        var args = new CommandArgs
        {
            Arguments = GetCommandArgs("echo Hello World"),
            CaptureOutput = true,
            ShouldIncludeTimestamp = true
        };

        // Act
        var result = await commandExecutor.ExecuteCommandAsync(args, CancellationToken.None);

        // Assert
        Assert.Matches(@"\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\]", result);
        Assert.Contains("Hello World", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithLogPrefix_IncludesPrefixInOutput()
    {
        // Arrange
        var commandExecutor = new CommandExecutorService(GetCommand());
        var args = new CommandArgs
        {
            Arguments = GetCommandArgs("echo Hello World"),
            CaptureOutput = true,
            LogPrefix = "TEST"
        };

        // Act
        var result = await commandExecutor.ExecuteCommandAsync(args, CancellationToken.None);

        // Assert
        Assert.Contains("[TEST]", result);
        Assert.Contains("Hello World", result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithoutCaptureOutput_ReturnsEmptyString()
    {
        // Arrange
        var commandExecutor = new CommandExecutorService(GetCommand());
        var args = new CommandArgs
        {
            Arguments = GetCommandArgs("echo Hello World"),
            CaptureOutput = false
        };

        // Act
        var result = await commandExecutor.ExecuteCommandAsync(args, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteCommandAsync_CommandWithError_ThrowsCommandExecutorException()
    {
        // Arrange
        var commandExecutor = new CommandExecutorService(GetCommand());
        var args = new CommandArgs
        {
            Arguments = GetCommandArgs("ls /nonexistentdirectory"),
            CaptureOutput = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutorException>(() => 
            commandExecutor.ExecuteCommandAsync(args, CancellationToken.None));
        
        Assert.NotEqual(0, exception.ExitCode);
        Assert.NotEmpty(exception.StdErr);
    }

    [Fact]
    public async Task ExecuteCommandAsync_CancellationRequested_KillsProcess()
    {
        // Arrange
        var commandExecutor = new CommandExecutorService("ping");
        var args = new CommandArgs
        {
            Arguments = "-t microsoft.com",
            CaptureOutput = true
        };

        using var cts = new CancellationTokenSource();

        // Act
        var task = commandExecutor.ExecuteCommandAsync(args, cts.Token);
        
        // Cancel after a small delay
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        cts.Cancel();
    }

    [Fact]
    public async Task ExecuteCommandAsync_LongRunningProcess_OutputsContinuously()
    {
        // Arrange
        // Create a file with test content to use for a longer-running command
        File.WriteAllText(_tempFilePath, string.Join(Environment.NewLine, Enumerable.Range(1, 100).Select(i => $"Line {i}")));
        
        var commandExecutor = new CommandExecutorService(GetCommand());
        var args = new CommandArgs
        {
            Arguments = GetCommandArgs($"type {_tempFilePath}"),
            CaptureOutput = true
        };

        // Act
        var result = await commandExecutor.ExecuteCommandAsync(args, CancellationToken.None);

        // Assert
        Assert.Contains("Line 1", result);
        Assert.Contains("Line 100", result);
        
        // Verify line count (should be at least 100 lines)
        var lineCount = result.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Length;
        Assert.True(lineCount >= 100, $"Expected at least 100 lines, but got {lineCount}");
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithErrorOutput_CapturesErrorOutput()
    {
        // Arrange
        // Use a command that generates error output
        var commandExecutor = new CommandExecutorService(GetCommand());
        var args = new CommandArgs
        {
            Arguments = GetCommandArgs("FINDSTR /notarealswitch something"),
            CaptureOutput = true
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CommandExecutorException>(() => 
            commandExecutor.ExecuteCommandAsync(args, CancellationToken.None));
        
        Assert.NotEmpty(exception.StdErr);
        Assert.Contains("FINDSTR", exception.StdErr.ToUpperInvariant());
    }

    // Helper method to get the correct command name based on OS
    private string GetCommand() => Environment.OSVersion.Platform == PlatformID.Win32NT ? "cmd.exe" : "/bin/bash";

    private string GetCommandArgs(string args) => Environment.OSVersion.Platform == PlatformID.Win32NT ? $"/c {args}" : $"-c \"{args}\"";

    public void Dispose()
    {
        // Clean up temp file
        if (File.Exists(_tempFilePath))
        {
            try
            {
                File.Delete(_tempFilePath);
            }
            catch
            {
                // Ignore failures during cleanup
            }
        }
    }
}