namespace trello_backuper.cli.WebHook
{
    public class WebHookData
    {
        public WebHookData(string message, HttpMethod method, Uri? targetUri)
        {
            Message = message;
            Method = method;
            TargetUri = targetUri;
        }

        public string Message { get; }

        public HttpMethod Method { get; }

        public Uri? TargetUri { get; }
    }
}
