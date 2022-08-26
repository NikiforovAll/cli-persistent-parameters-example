using System.CommandLine;

using Microsoft.Extensions.Configuration;

namespace CliStore;

public static class ConfigCommandsFactory
{
    static void Greet(string target) => Console.WriteLine($"Hello, {target}");

    public static Command GreetFromConfigCommand(CliConfigurationProvider configProvider)
    {
        var command = new Command("greet-from-config", "Demonstrates how to use IConfiguration from DI container");
        var targetOption = new Option<string?>("--target");
        targetOption.IsRequired = false;
        command.AddOption(targetOption);
        command.SetHandler((string? target, IConfiguration configuration) =>
            Greet(target ?? configuration["core:target"]), targetOption, configProvider);

        return command;
    }

    public static Command GreetFromDefaultValueCommand(CliConfigurationProvider configProvider)
    {
        var command = new Command("greet-from-default-value", "Demonstrates how to provide default value to an option");
        var targetOptionWithDefault = new Option<string>("--target", getDefaultValue: () =>
        {
            // note, this is evaluate preemptively which may slow down autocompletion
            var configuration = configProvider.GetConfiguration();
            return configuration["core:target"] ?? "<blank>";
        });
        command.AddOption(targetOptionWithDefault);
        command.SetHandler((string target) => Greet(target), targetOptionWithDefault);

        return command;
    }

    public static Command GreetFromPersistedCommand(CliConfigurationProvider configProvider)
    {
        var command = new Command("greet-from-persisted", "Demonstrates how to use persisted parameters");
        var targetOption = new Option<string>("--target");
        targetOption.IsRequired = false;
        command.AddOption(targetOption);
        configProvider.RegisterPersistedOption(targetOption);
        command.SetHandler(
            (string? target) => Greet(target!),
            new PersistedOptionProvider<string>(targetOption, configProvider));

        return command;
    }

}

