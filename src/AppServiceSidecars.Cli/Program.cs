using System.CommandLine;
using System.Threading.Tasks;
using AppServiceSidecarsCli.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.Builder;
using AppServiceSidecars.Core.Services;
using System.CommandLine.Parsing;
using AppServiceSidecars.Core.Services.Docker;
using System.Text;
using System;
using AppServiceSidecars.Core.Services.Logger;

namespace AppServiceSidecarsCli;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var rootCommand = new RootCommand("Sidecar CLI Tool")
        {
            new UpCommand(),
            new DownCommand(),
            new LogsCommand(),
            new BuildCommand()
        };

        var builder = new CommandLineBuilder(rootCommand)
                         .UseDefaults()
                         .UseDependencyInjection(services =>
                            {
                                services.AddSingleton<ISidecarService, SidecarService>();
                                services.AddSingleton<IDockerService, DockerService>();
                            })
                         .AddPrerequisitesCheckerMiddleware() ;

        LoggerService.AddNewLine();

        return await builder.Build().InvokeAsync(args);
    }
}

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