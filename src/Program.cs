using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

using CliStore;
using static CliStore.ConfigCommandsFactory;

var root = new RootCommand(
    "Shows proof of concept of how to store persistent configuration in a CLI apps");
root.Name = "clistore";

var commandLineBuilder = new CommandLineBuilder(root);

root.AddConfigCommands(out var configProvider);

root.AddCommand(GreetFromConfigCommand(configProvider));
root.AddCommand(GreetFromDefaultValueCommand(configProvider));
root.AddCommand(GreetFromPersistedCommand(configProvider));

commandLineBuilder
    .AddPersistedParametersMiddleware(configProvider)
    .UseDefaults();

var parser = commandLineBuilder.Build();

await parser.InvokeAsync(args);
