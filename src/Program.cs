using System.CommandLine;

var root = new RootCommand(
    "Shows proof of concept of how to store persistent configuration in a CLI apps");
root.Name = "clistore";

root.AddConfigCommands(out var configProvider);

var globalOption = new Option<string>("target", getDefaultValue: () =>
{
    // note, this is evaluate preemptively which may slow down autocompletion
    var configuration = configProvider.GetConfiguration();
    return configuration["core:target"] ?? "<blank>";
});
root.AddOption(globalOption);
// root.AddGlobalOption(globalOption);

root.SetHandler((string target) =>
{
    Console.WriteLine($"Hello, {target}");
}, globalOption);

await root.InvokeAsync(args);
