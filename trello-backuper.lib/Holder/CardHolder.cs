using trello_backuper.lib.Trello;
using trello_backuper.lib.Trello.Model;

namespace trello_backuper.lib.Holder;

public class CardHolder
{
    public CardHolder(TrelloCard card, TrelloApiResult<TrelloAttachment> attachments, TrelloApiResult<TrelloChecklist> checklists, TrelloApiResult<TrelloCardActions> comments)
    {
        Card = card;
        Attachments = attachments;
        Checklists = checklists;
        Comments = comments;
    }

    public TrelloCard Card { get; }

    public TrelloApiResult<TrelloAttachment> Attachments { get; }

    public TrelloApiResult<TrelloChecklist> Checklists { get; }

    public TrelloApiResult<TrelloCardActions> Comments { get; }
}