using trello_backuper.lib.Trello.Model;

namespace trello_backuper.lib.Holder;

public class ListHolder
{
    private readonly List<CardHolder> _cardHolders = new();

    public ListHolder(TrelloList list)
    {
        List = list;
    }

    public TrelloList List { get; }

    public IEnumerable<CardHolder> Cards => _cardHolders;

    public void AddCardHolder(CardHolder cardHolder)
    {
        _cardHolders.Add(cardHolder);
    }
}