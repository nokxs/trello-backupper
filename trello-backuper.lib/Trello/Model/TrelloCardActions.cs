namespace trello_backuper.lib.Trello.Model;

public class TrelloCardActions
{
    public string Id { get; set; }

    public string Type { get; set; }

    public TrelloCardActionData Data { get; set; }

    public TrelloCardActionMemberCreator MemberCreator { get; set; }
}