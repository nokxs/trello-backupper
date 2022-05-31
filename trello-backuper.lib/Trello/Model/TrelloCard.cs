namespace trello_backuper.lib.Trello.Model;

public class TrelloCard
{
    public string Id { get; set; }

    public string Name { get; set; }

    public bool Closed { get; set; }

    public string IdBoard { get; set; }

    public IEnumerable<string> IdChecklists { get; set; } = Enumerable.Empty<string>();

    public string IdList { get; set; }

    public Uri Url { get; set; }

    public Uri ShortUrl { get; set; }

    public string Desc { get; set; }

    public IEnumerable<TrelloCardLabel> Labels { get; set; } = Enumerable.Empty<TrelloCardLabel>();

    public TrelloCardBadges Badges { get; set; }
}