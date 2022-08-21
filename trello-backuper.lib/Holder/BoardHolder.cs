using trello.backuper.lib.Trello.Model;

namespace trello.backuper.lib.Holder;

public class BoardHolder
{
    private readonly List<ListHolder> _listHolders = new();

    public BoardHolder(TrelloBoard board)
    {
        Board = board;
    }

    public TrelloBoard Board { get; }

    public IEnumerable<ListHolder> Lists => _listHolders;

    public void AddListHolder(ListHolder listHolder)
    {
        _listHolders.Add(listHolder);
    }
}