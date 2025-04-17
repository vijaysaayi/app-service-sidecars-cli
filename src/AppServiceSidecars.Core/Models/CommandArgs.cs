namespace AppServiceSidecars.Core.Models;

public record CommandArgs
{
    public string Arguments { get; init; } = string.Empty;

    public bool CaptureOutput { get; init; }

    public string LogPrefix { get; init; } = string.Empty;

    public bool IsDebugMode { get; init; }

    public bool ShouldIncludeTimestamp { get; init; }

    public CancellationToken CancellationToken { get; init; } = default;

    public bool LogToConsole { get; init; } = true;

    public string? WorkingDirectory { get; set; }
}
