using Spectre.Console;

namespace AppServiceSidecars.Core.Services.Logger;

public class LoggerService
{
    public static void Info(string message)
    {
        AnsiConsole.MarkupLine($"[white]{Markup.Escape(message)}[/]");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
    }
}
