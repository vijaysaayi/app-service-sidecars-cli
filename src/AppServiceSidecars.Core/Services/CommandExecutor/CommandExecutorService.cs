using AppServiceSidecars.Core.Models;
using AppServiceSidecars.Core.Services.Logger;
using System.Diagnostics;
using System.Text;

namespace AppServiceSidecars.Core.Services.CommandExecutor;

public class CommandExecutorService
{
    private readonly string _command;

    public CommandExecutorService(string command) => _command = command;

    public async Task<string> ExecuteCommandAsync(CommandArgs args, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _command,
            Arguments = args.Arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using (cancellationToken.Register(() => { if (!process.HasExited) TryKillProcess(process); }))
        {
            process.OutputDataReceived += (sender, e) => HandleOutput(e.Data, args, outputBuilder, LoggerService.Info);
            process.ErrorDataReceived += (sender, e) => HandleOutput(e.Data, args, errorBuilder, LoggerService.Error);

            var tcs = new TaskCompletionSource<bool>();
            process.Exited += (sender, e) => tcs.TrySetResult(true);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cancellationToken));
            process.WaitForExit();

            if (process.ExitCode != 0 && !cancellationToken.IsCancellationRequested)
            {
                throw new CommandExecutorException("Command Execution Failed", process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
            }

            return args.CaptureOutput ? outputBuilder.ToString().Trim() : string.Empty;
        }
    }

    private void HandleOutput(string? data, CommandArgs args, StringBuilder outputBuilder, Action<string> logAction)
    {
        if (data == null) return;

        var prefix = BuildLogPrefix(args);
        var outputLine = prefix + data;

        if (args.LogToConsole)
        {
            logAction(outputLine);
        }        

        if (args.CaptureOutput)
        {
            outputBuilder.AppendLine(outputLine);
        }
    }

    private static string BuildLogPrefix(CommandArgs args)
    {
        if (!args.ShouldIncludeTimestamp && string.IsNullOrWhiteSpace(args.LogPrefix)) return string.Empty;

        var timestamp = args.ShouldIncludeTimestamp ? $"[{DateTime.Now:yyyy-MM-dd\\THH:mm:ss}]" : string.Empty;
        var logPrefix = !string.IsNullOrWhiteSpace(args.LogPrefix) ? $"[{args.LogPrefix}]" : string.Empty;

        return $"{timestamp}{logPrefix} : ";
    }

    private static void TryKillProcess(Process process)
    {
        try { process.Kill(); } catch { /* ignore */ }
    }
}
