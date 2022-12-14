using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Text.Json;

using Microsoft.Extensions.Configuration;

namespace System.CommandLine;

public static class ConfigCommands
{
    public static RootCommand AddConfigCommands(
        this RootCommand root,
        out CliConfigurationProvider configurationProvider)
    {
        var command = new Command("config", "Manage CLI configuration");

        configurationProvider = CliConfigurationProvider.Create(root.Name);
        command.AddCommand(BuildGetCommand(configurationProvider));
        command.AddCommand(BuildSetCommand(configurationProvider));
        command.AddCommand(BuildUnsetCommand(configurationProvider));

        root.AddCommand(command);
        return root;
    }

    private static Command BuildGetCommand(CliConfigurationProvider configurationProvider)
    {
        var get = new Command("get", "Get a configuration");
        var getpath = new Argument<string?>(
            "key",
            () => default,
            @"The configuration to get. If not provided, all sections and configurations
will be listed. If `section` is provided, all configurations under the
specified section will be listed. If `<section>.<key>` is provided, only
the corresponding configuration is shown.");
        get.AddArgument(getpath);

        get.SetHandler((string? path, IConfiguration configuration) =>
        {
            // TODO: implement filtering by section, section+key
            var output = new Dictionary<string, object[]>();

            foreach (var config in configuration.GetChildren())
            {
                output[config.Key] = config.GetChildren()
                    .Select(x => new { Name = x.Key, x.Value })
                    .ToArray();
            }

            if (output.Any())
            {
                Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }));
            }

            return Task.CompletedTask;
        }, getpath, configurationProvider);

        return get;
    }

    private static Command BuildSetCommand(CliConfigurationProvider configurationProvider)
    {
        var set = new Command("set", "Set a configuration");
        var setpath = new Argument<string[]>(
            "key",
            "Space-separated configurations in the form of <section>.<key>=<value>.");
        set.AddArgument(setpath);

        set.SetHandler((string[] path, IConfiguration configuration) =>
        {
            var ini = configurationProvider.LoadIniFile();

            foreach (var p in path)
            {
                var keyvalue = p.Split('=');
                var (key, value) = (keyvalue[0], keyvalue[^1]);

                var sectionKey = key[..key.IndexOf('.')];
                var configKey = key[(key.IndexOf('.') + 1)..];
                ini[sectionKey][configKey] = value;
            }
            ini.Save(configurationProvider.ConfigLocation);
            return Task.CompletedTask;
        }, setpath, configurationProvider);

        return set;
    }

    private static Command BuildUnsetCommand(CliConfigurationProvider configurationProvider)
    {
        // TODO: unset section?
        var unset = new Command("unset", "Unset a configuration");

        var unsetpath = new Argument<string[]>(
            "key",
            "The configuration to unset, in the form of <section>.<key>.");
        unset.AddArgument(unsetpath);

        unset.SetHandler((string[] path, IConfiguration configuration) =>
        {
            var ini = configurationProvider.LoadIniFile();

            foreach (var p in path)
            {
                var sectionKey = p[..p.IndexOf('.')];
                var configKey = p[(p.IndexOf('.') + 1)..];
                ini[sectionKey].Remove(configKey);
            }
            ini.Save(configurationProvider.ConfigLocation);

            return Task.CompletedTask;
        }, unsetpath, configurationProvider);

        return unset;
    }
}

public class CliConfigurationProvider : BinderBase<IConfiguration>
{
    public const string PersistedParamsSection = "persisted_params";

    public IEnumerable<Option> PersistedOptions => persistedOptions.AsEnumerable();
    private IList<Option> persistedOptions = new List<Option>();
    public static CliConfigurationProvider Create(string storeName = "clistore") =>
        new(storeName);

    public CliConfigurationProvider(string storeName) => StoreName = storeName;
    public string StoreName { get; }
    public string ConfigLocationDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        $".{StoreName.TrimStart('.')}");

    public string ConfigLocation => Path.Combine(ConfigLocationDir, "config");

    protected override IConfiguration GetBoundValue(
        BindingContext bindingContext) => GetConfiguration();

    public IConfiguration GetConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(StoreName.ToUpperInvariant())
            .AddIniFile(ConfigLocation, optional: true)
            .Build();

        return configuration;
    }

    public void RegisterPersistedOption(Option option) => persistedOptions.Add(option);

    public IniFile LoadIniFile()
    {
        var ini = new IniFile();

        Directory.CreateDirectory(ConfigLocationDir);

        if (File.Exists(ConfigLocation))
        {
            ini.Load(ConfigLocation);
        }

        return ini;
    }
}

public class PersistedOptionProvider<T> : BinderBase<T?>
{
    public PersistedOptionProvider(Option<T> option, CliConfigurationProvider configProvider)
    {
        _option = option;
        _configProvider = configProvider;
        configProvider.RegisterPersistedOption(option);
    }

    protected override T? GetBoundValue(BindingContext bindingContext)
    {
        if (!bindingContext.ParseResult.HasOption(_option))
        {
            var ini = _configProvider.LoadIniFile();
            string text = ini[CliConfigurationProvider.PersistedParamsSection][_option.Name].ToString();
            var value = (T)TypeDescriptor.GetConverter(typeof(T))
                .ConvertFromString(text)!;

            return value;
        }

        return bindingContext.ParseResult.GetValueForOption(_option);
    }

    private readonly Option<T> _option;
    private readonly CliConfigurationProvider _configProvider;
}

