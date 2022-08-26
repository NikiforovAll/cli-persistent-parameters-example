using System.Text;

using CliWrap;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CliStore.Tests;

[UsesVerify]
public class StoreCommands_Specs
{
    private const string relativeSourcePath = "../../../../../src";

    public StoreCommands_Specs()
    {
        // cleanup, runs every test, concurrent test execution is disabled
        EnsureDeletedConfigFolder();
    }

    [Fact]
    public async Task Help_text_is_displayed_for_root_command()
    {
        var stdOutBuffer = Execute("--help");

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Help_text_is_displayed_for_config()
    {
        var stdOutBuffer = Execute("config", "--help");

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Help_text_is_displayed_for_config_get()
    {
        var stdOutBuffer = Execute("config", "get", "--help");

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task GreetFromConfig_greets_with_populated_target_config()
    {
        Execute("config", "set", "core.target=World");
        var stdOutBuffer = Execute("greet-from-config");

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task GreetFromDefaultValue_greets_with_default_value_from_populated_target_config()
    {
        Execute("config", "set", "core.target=World");
        var stdOutBuffer = Execute("greet-from-default-value");

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task GreetFromPersisted_greets_with_persisted_params_value()
    {
        Execute("greet-from-persisted", "--target", "World");
        var stdOutBuffer = Execute("greet-from-persisted");

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Get_config_is_performed_on_empty_config_folder()
    {
        var stdOutBuffer = Execute("config", "get");

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Get_config_is_performed_on_populated_config()
    {
        Execute("config", "set", "core.target=my_value");
        Execute("config", "set", "core.is_populated=true");
        Execute("config", "set", "extra.another_section=false");
        var stdOutBuffer = Execute("config", "get");

        await Verify(stdOutBuffer.ToString());
    }

    private static StringBuilder Execute(params string[] command)
    {
        var (_, stdOutBuffer, _) = ExecuteDetailedResult(command);
        
        return stdOutBuffer;
    }

    private static (CommandResult, StringBuilder, StringBuilder) ExecuteDetailedResult(params string[] command)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        var result = Cli.Wrap("dotnet")
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .WithArguments(args => args
                .Add("run")
                .Add("--project")
                .Add(relativeSourcePath)
                .Add("--")
                .Add(command))
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        return (result, stdOutBuffer, stdErrBuffer);
    }

    private static void EnsureDeletedConfigFolder()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".clistore",
            "config");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
