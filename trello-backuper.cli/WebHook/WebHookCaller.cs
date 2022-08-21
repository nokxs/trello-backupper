using Microsoft.Extensions.Logging;

namespace trello_backuper.cli.WebHook;

public class WebHookCaller
{
    private readonly ILogger<WebHookCaller> _logger;
    private readonly HttpClient _httpClient = new();

    public WebHookCaller(ILogger<WebHookCaller> logger)
    {
        _logger = logger;
    }

    public async Task TrySend(WebHookData data)
    {
        if (data.TargetUri == null)
        {
            return;
        }

        try
        {
            var finalUri = new Uri(data.TargetUri, $"?message={data.Message}");
            _logger.LogInformation($"Calling {finalUri} with method {data.Method}");

            var request = new HttpRequestMessage(data.Method, finalUri);
            var result = await _httpClient.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                _logger.LogInformation("Web hook call was successful.");
            }
            else
            {
                _logger.LogError(
                    $"Web hook call was not successful. Error is ignored. " +
                    $"Status code: {result.StatusCode}. Reason: {result.ReasonPhrase}");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Web hook call was not successful. Error is ignored.");
        }
    }
}