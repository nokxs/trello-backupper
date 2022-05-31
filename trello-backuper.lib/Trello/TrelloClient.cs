using System.Net.Http.Headers;
using System.Text.Json;
using trello_backuper.lib.Trello.Model;

namespace trello_backuper.lib.Trello;

public class TrelloClient
{
    private const string TrelloApiBaseUrl = "https://api.trello.com/1";

    private readonly string _appKey;
    private readonly string _token;

    private readonly HttpClient _httpClient = new();

    public TrelloClient(string appKey, string token)
    {
        _appKey = appKey;
        _token = token;
    }

    public async Task<TrelloApiResult<TrelloBoard>> GetBoardsAsync()
    {
        return await GetFromApi<TrelloBoard>("members/me/boards");
    }

    public async Task<TrelloApiResult<TrelloList>> GetListsAsync(TrelloBoard board)
    {
        return await GetFromApi<TrelloList>($"boards/{board.Id}/lists");
    }

    public async Task<TrelloApiResult<TrelloCard>> GetCardsAsync(TrelloList list)
    {
        return await GetFromApi<TrelloCard>($"lists/{list.Id}/cards");
    }

    public async Task<TrelloApiResult<TrelloAttachment>> GetCardAttachmentsAsync(TrelloCard card)
    {
        if (card.Badges.Attachments > 0)
        {
            return await GetFromApi<TrelloAttachment>($"cards/{card.Id}/attachments");
        }

        return new TrelloApiResult<TrelloAttachment>();
    }

    public async Task<TrelloApiResult<TrelloChecklist>> GetCardChecklistsAsync(TrelloCard card)
    {
        if (card.IdChecklists.Any())
        {
            return await GetFromApi<TrelloChecklist>($"cards/{card.Id}/checklists");
        }

        return new TrelloApiResult<TrelloChecklist>();
    }

    public async Task<TrelloApiResult<TrelloCardActions>> GetCardComments(TrelloCard card)
    {
        if (card.Badges.Comments > 0)
        {
            var result = await GetFromApi<TrelloCardActions>($"cards/{card.Id}/actions");
            return new TrelloApiResult<TrelloCardActions>(result.RawJson, result.Result.Where(action => action.Type == "commentCard"));
        }

        return new TrelloApiResult<TrelloCardActions>();
    }

    public async Task<TrelloApiResult<TResult>> GetFromApi<TResult>(string apiPath)
    {
        var rawJson = await _httpClient.GetStringAsync($"{TrelloApiBaseUrl}/{apiPath}?{GetAuthParameters()}");
        var result = JsonSerializer.Deserialize<IEnumerable<TResult>>(rawJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? Enumerable.Empty<TResult>();
        return new TrelloApiResult<TResult>(rawJson, result);
    }

    public async Task DownloadAttachment(TrelloAttachment attachment, TrelloCredentials credentials, string directory)
    {
        if (attachment.IsUpload)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", $"oauth_consumer_key=\"{credentials.AppKey}\", oauth_token=\"{credentials.Token}\"");
            var response = await httpClient.GetStreamAsync(attachment.Url);

            await using var fs = new FileStream(GetAttachmentFilePath(attachment, directory), FileMode.CreateNew);
            await response.CopyToAsync(fs);
        }
    }

    private static string GetAttachmentFilePath(TrelloAttachment trelloAttachment, string directory)
    {
        var path = Path.Combine(directory, trelloAttachment.FileName);

        if (File.Exists(path))
        {
            return Path.Combine(directory, $"{Guid.NewGuid()}-{trelloAttachment.FileName}");
        }

        return path;
    }

    private string GetAuthParameters()
    {
        return $"key={_appKey}&token={_token}";
    }
}

public class TrelloApiResult<TResult>
{
    public TrelloApiResult() : this(string.Empty, Enumerable.Empty<TResult>())
    {
    }

    public TrelloApiResult(string rawJson, IEnumerable<TResult> result)
    {
        RawJson = rawJson;
        Result = result;
    }

    public string RawJson { get; }

    public IEnumerable<TResult> Result { get; }
}