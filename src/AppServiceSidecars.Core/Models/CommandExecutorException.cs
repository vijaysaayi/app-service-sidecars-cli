namespace AppServiceSidecars.Core.Models;

public class CommandExecutorException(string message, int exitCode, string stdOut, string stdErr) : Exception(message)
{
    public int ExitCode { get; private set; } = exitCode;

    public string StdOut { get; private set; } = stdOut;

    public string StdErr { get; private set; } = stdErr;
}
