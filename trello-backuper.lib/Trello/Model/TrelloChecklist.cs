namespace trello.backuper.lib.Trello.Model;

public class TrelloChecklist
{
    public string Id { get; set; } = "";

    public string Name { get; set; } = "";

    public string IdBoard { get; set; } = "";

    public string IdCard { get; set; } = "";

    public IEnumerable<TrelloChecklistItem> CheckItems { get; set; } = Enumerable.Empty<TrelloChecklistItem>();
}