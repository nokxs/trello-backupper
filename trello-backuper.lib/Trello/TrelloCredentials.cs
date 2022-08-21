namespace trello.backuper.lib.Trello
{
    public class TrelloCredentials
    {
        public TrelloCredentials(string appKey, string token)
        {
            AppKey = appKey;
            Token = token;
        }
        public string AppKey { get; }

        public string Token { get; }
    }
}
