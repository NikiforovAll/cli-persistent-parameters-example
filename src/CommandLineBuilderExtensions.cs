using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace CliStore;

public static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder AddPersistedParametersMiddleware(
        this CommandLineBuilder builder, CliConfigurationProvider configProvider)
    {
        return builder.AddMiddleware(async (context, next) =>
        {
            Lazy<IniFile> config = new(() => configProvider.LoadIniFile());
            var parseResult = context.ParseResult;

            await next(context);

            bool newValuesAdded = false;
            foreach (var option in configProvider.PersistedOptions)
            {
                if (parseResult.HasOption(option))
                {
                    config.Value[CliConfigurationProvider.PersistedParamsSection][option.Name] =
                        parseResult.GetValueForOption(option)?.ToString();
                    newValuesAdded = true;
                }
            }

            if (newValuesAdded)
            {
                config.Value.Save(configProvider.ConfigLocation);
            }
        });
    }
}
