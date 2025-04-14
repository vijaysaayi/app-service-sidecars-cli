using System.CommandLine;
using System.Threading.Tasks;
using AppServiceSidecarsCli.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.Builder;
using AppServiceSidecars.Core.Services;
using System.CommandLine.Parsing;

namespace AppServiceSidecarsCli;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Sidecar CLI Tool")
        {
            new UpCommand(),
            new DownCommand(),
            new LogsCommand()
        };

        var builder = new CommandLineBuilder(rootCommand).UseDefaults().UseDependencyInjection(services =>
        {
            services.AddSingleton<ISidecarService, SidecarService>();
        });

        return await builder.Build().InvokeAsync(args);
    }
}

// Move the extension method to a non-generic static class
internal static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder UseDefaults(this CommandLineBuilder builder)
    {
        return builder
            .UseVersionOption()
            .UseHelp()
            .UseEnvironmentVariableDirective()
            .UseParseDirective()
            .UseSuggestDirective()
            .RegisterWithDotnetSuggest()
            .UseTypoCorrections()
            .UseParseErrorReporting()
            .UseExceptionHandler()
            .CancelOnProcessTermination();
    }
}