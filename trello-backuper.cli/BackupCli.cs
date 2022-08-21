using System.CommandLine;

namespace trello.backupper.cli;

public class BackupCli
{
    private readonly BackupCommand _backupCommand;

    public BackupCli(BackupCommand backupCommand)
    {
        _backupCommand = backupCommand;
    }

    public async Task<int> Run(string[] args)
    {
        var appKeyOption = new Option<string>(
            name: "--app-key",
            description: "The app key for trello");

        var tokenOption = new Option<string>(
            name: "--token",
            description: "The token for trello");

        var rootCommand = new RootCommand("Create a backup of all trello boards");
        rootCommand.AddOption(appKeyOption);
        rootCommand.AddOption(tokenOption);

        rootCommand.AddCommand(_backupCommand.CreateCommand(appKeyOption, tokenOption));

        return await rootCommand.InvokeAsync(args);
    }
}