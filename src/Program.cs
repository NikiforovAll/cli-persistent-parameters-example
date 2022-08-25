using System.CommandLine;

using Microsoft.Extensions.Configuration;

var root = new RootCommand(
    "Shows proof of concept of how to store persistent configuration in a CLI apps");
root.Name = "clistore";

root.AddConfigCommands(out var configProvider);

root.SetHandler((IConfiguration configuration) =>
{
    Console.WriteLine($"Hello, {configuration["core:target"] ?? "<blank>"}");
}, configProvider);

await root.InvokeAsync(args);
