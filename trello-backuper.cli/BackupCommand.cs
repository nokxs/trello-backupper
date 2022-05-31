using System.CommandLine;
using trello_backuper.lib;

namespace trello_backuper.cli;

public class BackupCommand
{
    private readonly BackupCreator _backupCreator;

    public BackupCommand(BackupCreator backupCreator)
    {
        _backupCreator = backupCreator;
    }

    public Command CreateCommand(Option appKeyOption, Option tokenOption)
    {
        var backupCommand = new Command("backup", "Create a backup of all trello boards");
        var targetDirectoryArgument = new Argument<string>("targetDirectory", "The target directory to save the backup to");

        var skipAttachmentDownloadOption = new Option<bool>("--skip-attachment-download", "Skip the download of attachments");
        var skipJsonBackupOption = new Option<bool>("--skip-json-backup", "Skip saving json files");
        var skipHtmlCreationOption = new Option<bool>("--skip-html-creation", "Skip the creation of the html file per board");

        backupCommand.AddArgument(targetDirectoryArgument);
        backupCommand.AddOption(skipAttachmentDownloadOption);
        backupCommand.AddOption(skipJsonBackupOption);
        backupCommand.AddOption(skipHtmlCreationOption);

        backupCommand.SetHandler(BackupCommandHandler(),
            appKeyOption, tokenOption, skipAttachmentDownloadOption, skipJsonBackupOption, skipHtmlCreationOption, targetDirectoryArgument);


        return backupCommand;
    }

    private Func<string, string, bool, bool, bool, string, Task> BackupCommandHandler()
    {
        return async (appKey, token, skipAttachmentDownload, skipJsonBackup, skipHtmlCreation, targetDirectory) =>
        {
            await foreach (var board in _backupCreator.CreateModel(appKey, token, (name, json) => DownloadedJson(name, json, targetDirectory, skipJsonBackup)))
            {
                var markdown = _backupCreator.CreateMarkdown(board);

                var boardTargetDirectory = Path.Combine(targetDirectory, MakeValidFileName(board.Board.Name));
                Directory.CreateDirectory(boardTargetDirectory);

                var markdownPath = Path.Combine(boardTargetDirectory, MakeValidFileName($"{board.Board.Name}.md"));
                await File.WriteAllTextAsync(markdownPath, markdown);

                if (!skipHtmlCreation)
                {
                    var html = _backupCreator.CreateHtmlFromMarkdown(markdown);
                    var htmlPath = Path.Combine(boardTargetDirectory, MakeValidFileName($"{board.Board.Name}.html"));
                    await File.WriteAllTextAsync(htmlPath, html);
                }

                if (!skipAttachmentDownload)
                {
                    var attachmentPath = Path.Combine(boardTargetDirectory, "attachments");
                    Directory.CreateDirectory(attachmentPath);
                    await _backupCreator.DownloadAllAttachments(appKey, token, attachmentPath, board);
                }
            }
        };
    }

    private void DownloadedJson(string name, string json, string targetDirectory, bool skipJsonBackup)
    {
        if (skipJsonBackup || string.IsNullOrEmpty(json))
        {
            return;
        }

        var jsonDirectory = Path.Combine(targetDirectory, "json");
        Directory.CreateDirectory(jsonDirectory);

        var jsonFilePath = Path.Combine(jsonDirectory, MakeValidFileName($"{name}.json"));
        File.WriteAllText(jsonFilePath, json);
    }

    private static string MakeValidFileName(string name)
    {
        string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }
}