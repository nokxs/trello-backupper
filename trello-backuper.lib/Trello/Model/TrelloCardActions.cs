namespace trello_backuper.lib.Trello.Model;

public class TrelloCardActions
{
    public string Id { get; set; } = "";

    public string Type { get; set; } = "";

    public TrelloCardActionData Data { get; set; } = new();

    public TrelloCardActionMemberCreator MemberCreator { get; set; } = new();
}