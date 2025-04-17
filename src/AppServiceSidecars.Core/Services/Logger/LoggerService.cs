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

    public static void DisplayBox(string message)
    {
        var text = new Text(message, new Style(Color.White))
        {
            Justification = Justify.Center
        };

        var panel = new Panel(text)
        {
            Border = BoxBorder.Ascii,
            Expand = true
        };

        AnsiConsole.Write(panel);
    }

    public static void AddNewLine()
    {
        AnsiConsole.WriteLine();
    }
}
