using System.Text;

using CliWrap;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CliStore.Tests;

[UsesVerify]
public class StoreCommands_Specs
{
    private const string relativeSourcePath = "../../../../../src";

    [Fact]
    public async Task Help_text_is_displayed_for_root_command()
    {
        ExecuteCLI(new string[] { "--help" }, out var stdOutBuffer, out _);

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Help_text_is_displayed_for_config()
    {
        ExecuteCLI(new string[] { "config", "--help" }, out var stdOutBuffer, out _);

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Help_text_is_displayed_for_config_get()
    {
        ExecuteCLI(new string[] { "config", "get", "--help" }, out var stdOutBuffer, out _);

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task RootCommand_greets_with_populated_target_config()
    {
        EnsureDeletedConfigFolder();
        ExecuteCLI(new string[] { "config", "set", "core.target=World" }, out var _, out _);
        ExecuteCLI(Array.Empty<string>(), out var stdOutBuffer, out _);

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Get_config_is_performed_on_empty_config_folder()
    {
        EnsureDeletedConfigFolder();
        ExecuteCLI(new string[] { "config", "get" }, out var stdOutBuffer, out _);

        await Verify(stdOutBuffer.ToString());
    }

    [Fact]
    public async Task Get_config_is_performed_on_populated_config()
    {
        EnsureDeletedConfigFolder();
        ExecuteCLI(new string[] { "config", "set", "core.target=my_value" }, out var _, out _);
        ExecuteCLI(new string[] { "config", "set", "core.is_populated=true" }, out var _, out _);
        ExecuteCLI(new string[] { "config", "set", "extra.another_section=false" }, out var _, out _);
        ExecuteCLI(new string[] { "config", "get" }, out var stdOutBuffer, out _);

        await Verify(stdOutBuffer.ToString());
    }

    private CommandResult ExecuteCLI(
        string[] command,
        out StringBuilder stdOutBuffer,
        out StringBuilder stdErrBuffer)
    {
        stdOutBuffer = new StringBuilder();
        stdErrBuffer = new StringBuilder();

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

        return result;
    }

    private static void EnsureDeletedConfigFolder() =>
        File.Delete(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".clistore",
            "config"));
}
