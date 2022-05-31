using Markdig;
using Microsoft.Extensions.Logging;
using trello_backuper.lib.Holder;
using trello_backuper.lib.Markdown;
using trello_backuper.lib.Trello;

namespace trello_backuper.lib
{
    public class BackupCreator
    {
        private readonly ILogger<BackupCreator> _logger;

        public BackupCreator(ILogger<BackupCreator> logger)
        {
            _logger = logger;
        }

        public async IAsyncEnumerable<BoardHolder> CreateModel(string appKey, string token, Action<string, string> downloadedJson)
        {
            var trelloClient = new TrelloClient(appKey, token);
            var boards = await trelloClient.GetBoardsAsync();
            downloadedJson("boards", boards.RawJson);

            foreach (var board in boards.Result)
            {
                _logger.LogInformation($"Starting to fetch board '{board.Name}'");

                var boardHolder = new BoardHolder(board);

                var lists = await trelloClient.GetListsAsync(board);
                downloadedJson($"{board.Name}_lists", lists.RawJson);
                foreach (var list in lists.Result)
                {
                    _logger.LogInformation($"Starting to fetch list '{list.Name}'");
                    var listHolder = new ListHolder(list);
                    boardHolder.AddListHolder(listHolder);

                    var cards = await trelloClient.GetCardsAsync(list);
                    downloadedJson($"{board.Name}_{list.Name}_cards", cards.RawJson);
                    foreach (var card in cards.Result)
                    {
                        _logger.LogInformation($"Starting to fetch card and its details '{card.Name}'");

                        var attachments = await trelloClient.GetCardAttachmentsAsync(card);
                        downloadedJson($"{board.Name}_{list.Name}_{card.Name}_attachments", attachments.RawJson);
                        var checklists = await trelloClient.GetCardChecklistsAsync(card);
                        downloadedJson($"{board.Name}_{list.Name}_{card.Name}_checklists", checklists.RawJson);
                        var comments = await trelloClient.GetCardComments(card);
                        downloadedJson($"{board.Name}_{list.Name}_{card.Name}_comments", comments.RawJson);

                        var cardHolder = new CardHolder(card, attachments, checklists, comments);
                        listHolder.AddCardHolder(cardHolder);
                    }

                    _logger.LogInformation($"Finished fetching list '{list.Name}'");
                }

                _logger.LogInformation($"Finished fetching board '{board.Name}'");
                yield return boardHolder;
            }
        }

        public async Task DownloadAllAttachments(string appKey, string token, string attachmentPath, BoardHolder boardHolder)
        {
            var trelloClient = new TrelloClient(appKey, token);

            var uploadAttachments =
                from lists in boardHolder.Lists
                from cards in lists.Cards
                from attachment in cards.Attachments.Result
                where attachment.IsUpload
                select attachment;

            foreach (var attachment in uploadAttachments)
            {
                _logger.LogInformation($"Downloading file '{attachment.FileName}' ({attachment.Bytes} bytes) from {attachment.Url}");
                await trelloClient.DownloadAttachment(attachment, new TrelloCredentials(appKey, token), attachmentPath);
            }
        }

        public string CreateMarkdown(BoardHolder boardHolder)
        {
            var markdownCreator = new MarkdownCreator(boardHolder);
            return markdownCreator.Create();
        }

        public string CreateHtmlFromMarkdown(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder().UseBootstrap().UseAdvancedExtensions().Build();
            var html = Markdig.Markdown.ToHtml(markdown, pipeline);

            string htmlTemplate = @$"  <!DOCTYPE html>
<html lang=""en"">
<head>
  <title>Trello Backup</title>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css"">
  <script src=""https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js""></script>
  <script src=""https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js""></script>
</head>
<body>

<div class=""container"">
{html}
</div>

</body>
</html> ";

            return htmlTemplate;
        }
    }
}
