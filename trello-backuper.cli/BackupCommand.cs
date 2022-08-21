using System.CommandLine;
using trello.backuper.lib;
using trello.backuper.lib.Holder;
using trello.backupper.cli.WebHook;

namespace trello.backupper.cli;

public class BackupCommand
{
    private readonly BackupCreator _backupCreator;
    private readonly WebHookCaller _webHookCaller;

    public BackupCommand(BackupCreator backupCreator, WebHookCaller webHookCaller)
    {
        _backupCreator = backupCreator;
        _webHookCaller = webHookCaller;
    }

    public Command CreateCommand(Option appKeyOption, Option tokenOption)
    {
        var backupCommand = new Command("backup", "Create a backup of all trello boards");
        var targetDirectoryArgument = new Argument<string>("targetDirectory", "The target directory to save the backup to");

        var skipAttachmentDownloadOption = new Option<bool>("--skip-attachment-download", "Skip the download of attachments");
        var skipJsonBackupOption = new Option<bool>("--skip-json-backup", "Skip saving json files");
        var skipHtmlCreationOption = new Option<bool>("--skip-html-creation", "Skip the creation of the html file per board");
        var webHookUrlOption = new Option<string?>("--web-hook-url",
            "Call this url every time a back is started, finished or an error occurred. Uses GET-Method if not specified otherwise. See readme for details");
        var webHookMethodOption = new Option<string?>("--web-hook-http-method",
            "Sets the http method used for a web hook. Needs to be combined with --web-hook-url. Allowed methods: GET, POST, PUT, PATCH");

        backupCommand.AddArgument(targetDirectoryArgument);
        backupCommand.AddOption(skipAttachmentDownloadOption);
        backupCommand.AddOption(skipJsonBackupOption);
        backupCommand.AddOption(skipHtmlCreationOption);
        backupCommand.AddOption(webHookUrlOption);
        backupCommand.AddOption(webHookMethodOption);

        backupCommand.SetHandler(BackupCommandHandler(),
            appKeyOption,
            tokenOption,
            skipAttachmentDownloadOption,
            skipJsonBackupOption,
            skipHtmlCreationOption,
            webHookUrlOption,
            webHookMethodOption,
            targetDirectoryArgument);
        
        return backupCommand;
    }

    private Func<string, string, bool, bool, bool, string?, string?, string, Task> BackupCommandHandler()
    {
        return async (appKey,
            token,
            skipAttachmentDownload,
            skipJsonBackup,
            skipHtmlCreation,
            webHookUrl,
            webHookHttpMethod,
            targetDirectory) =>
        {
            var parsedWebHookHttpMethod = ParseWebHookHttpMethod(webHookHttpMethod);
            var parsedWebHookUrl = webHookUrl == null ? null : new Uri(webHookUrl);

            try
            {
                await _webHookCaller.TrySend(new WebHookData("Started backup", parsedWebHookHttpMethod, parsedWebHookUrl));

                await foreach (var board in _backupCreator.CreateModel(appKey, token, (name, json) => DownloadedJson(name, json, targetDirectory, skipJsonBackup)))
                {
                    await DoBackupOfBoard(board, targetDirectory, skipHtmlCreation, skipAttachmentDownload, appKey, token);
                }

                await _webHookCaller.TrySend(new WebHookData("Finished backup", parsedWebHookHttpMethod, parsedWebHookUrl));
            }
            catch (Exception e)
            {
                await _webHookCaller.TrySend(new WebHookData($"Error occurred during backup: {e.Message}", parsedWebHookHttpMethod, parsedWebHookUrl));
                throw;
            }
        };
    }

    private async Task DoBackupOfBoard(BoardHolder board, string targetDirectory, bool skipHtmlCreation,
        bool skipAttachmentDownload, string appKey, string token)
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

    private HttpMethod ParseWebHookHttpMethod(string? webHookHttpMethod)
    {
        if (string.IsNullOrEmpty(webHookHttpMethod))
        {
            return HttpMethod.Get;;
        }

        return new HttpMethod(webHookHttpMethod);
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