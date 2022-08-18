using System.CommandLine;
using trello_backuper.cli.Telegram;
using trello_backuper.lib;

namespace trello_backuper.cli;

public class BackupCommand
{
    private readonly BackupCreator _backupCreator;
    private readonly TelegramBot _telegramBot;

    public BackupCommand(BackupCreator backupCreator, TelegramBot telegramBot)
    {
        _backupCreator = backupCreator;
        _telegramBot = telegramBot;
    }

    public Command CreateCommand(Option appKeyOption, Option tokenOption)
    {
        var backupCommand = new Command("backup", "Create a backup of all trello boards");
        var targetDirectoryArgument = new Argument<string>("targetDirectory", "The target directory to save the backup to");

        var skipAttachmentDownloadOption = new Option<bool>("--skip-attachment-download", "Skip the download of attachments");
        var skipJsonBackupOption = new Option<bool>("--skip-json-backup", "Skip saving json files");
        var skipHtmlCreationOption = new Option<bool>("--skip-html-creation", "Skip the creation of the html file per board");
        var enableTelegram = new Option<string>("--enableTelegram", "Will send status messages over telegram. Requires a telegram bot api token");

        backupCommand.AddArgument(targetDirectoryArgument);
        backupCommand.AddOption(skipAttachmentDownloadOption);
        backupCommand.AddOption(skipJsonBackupOption);
        backupCommand.AddOption(skipHtmlCreationOption);
        backupCommand.AddOption(enableTelegram);

        backupCommand.SetHandler(BackupCommandHandler(),
            appKeyOption, tokenOption, skipAttachmentDownloadOption, skipJsonBackupOption, skipHtmlCreationOption, enableTelegram, targetDirectoryArgument);


        return backupCommand;
    }

    private Func<string, string, bool, bool, bool, string, string, Task> BackupCommandHandler()
    {
        return async (appKey, token, skipAttachmentDownload, skipJsonBackup, skipHtmlCreation, telegramToken, targetDirectory) =>
        {
            try
            {
                var startTime = DateTime.Now;
                var downloadedBoards = 0;

                if (!string.IsNullOrEmpty(telegramToken))
                {
                    _telegramBot.StartListening(telegramToken);
                    await _telegramBot.EnsureOneUserIsRegisteredAsync();
                    await _telegramBot.TrySendMessage("Starting backup of trello boards");
                }

                await foreach (var board in _backupCreator.CreateModel(
                                   appKey, 
                                   token,
                                   (name, json) => DownloadedJson(name, json, targetDirectory, skipJsonBackup)))
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

                    downloadedBoards++;
                }

                await _telegramBot.TrySendMessage($"Finished backup of {downloadedBoards} boards in {DateTime.Now - startTime}");
            }
            catch (Exception e)
            {
                await _telegramBot.TrySendMessage($"An error occurred during backup: {e.Message}");
                throw;
            }
        };
    }

    private static void DownloadedJson(string name, string json, string targetDirectory, bool skipJsonBackup)
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