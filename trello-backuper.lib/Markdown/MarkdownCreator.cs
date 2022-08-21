using System.Text;
using System.Text.RegularExpressions;
using trello.backuper.lib.Holder;
using trello.backuper.lib.Trello;
using trello.backuper.lib.Trello.Model;

namespace trello.backuper.lib.Markdown;

public class MarkdownCreator
{
    private readonly BoardHolder _boardHolder;

    public MarkdownCreator(BoardHolder boardHolder)
    {
        _boardHolder = boardHolder;
    }

    public string Create()
    {
        var sb = new StringBuilder();

        AddCommon(sb);

        foreach (var listHolder in _boardHolder.Lists)
        {
            sb.AppendLine(GetList(listHolder));
            AppendHorizontalRule(sb);
        }

        sb.AppendLine();
        var markdownDocument = sb.ToString();
        var cleanedUpMarkdownDocument = RemoveMultipleBlankLines(markdownDocument);
        return cleanedUpMarkdownDocument;
    }

    private static string GetList(ListHolder listHolder)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"## {listHolder.List.Name}");
        sb.AppendLine();

        foreach (var cardHolder in listHolder.Cards)
        {
            sb.AppendLine(GetCard(cardHolder));
        }

        return sb.ToString();
    }

    private static string GetCard(CardHolder cardHolder)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"### {cardHolder.Card.Name}");
        sb.AppendLine();

        if (cardHolder.Card.Labels.Any())
        {
            sb.AppendLine($"Labels: {GetLabels(cardHolder.Card.Labels)}");
        }

        if (!string.IsNullOrWhiteSpace(cardHolder.Card.Desc))
        {
            sb.AppendLine($"> {cardHolder.Card.Desc}");
        }

        foreach (var checklist in cardHolder.Checklists.Result)
        {
            sb.AppendLine(GetChecklist(checklist));
        }

        if (cardHolder.Card.Badges.Attachments > 0)
        {
            sb.AppendLine(GetAttachments(cardHolder.Attachments.Result));
        }

        if (cardHolder.Card.Badges.Comments > 0)
        {
            sb.AppendLine(GetComments(cardHolder.Comments));
        }

        return sb.ToString();
    }

    private static string GetLabels(IEnumerable<TrelloCardLabel> labels)
    {
        var sb = new StringBuilder();

        foreach (var label in labels)
        {
            sb.Append($"**{label.Name}**");
        }

        return sb.ToString();
    }

    private static string GetComments(TrelloApiResult<TrelloCardActions> cardActions)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#### Comments");
        sb.AppendLine();

        foreach (var action in cardActions.Result)
        {
            sb.AppendLine($"- {action.Data.Text}");
        }
        
        sb.AppendLine();
        return sb.ToString();
    }

    private static string GetAttachments(IEnumerable<TrelloAttachment> attachments)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#### Attachments");

        foreach (var attachment in attachments)
        {
            sb.AppendLine($"- [{attachment.Name}]({attachment.Url})");
        }

        sb.AppendLine();
        return sb.ToString();
    }

    private static string GetChecklist(TrelloChecklist checklist)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"#### Checklist '{checklist.Name}'");
        sb.AppendLine();

        foreach (var item in checklist.CheckItems)
        {
            var checkString = item.State == "complete" ? "X" : " ";
            var strikeThroughString = item.State == "complete" ? "~~" : string.Empty;
            sb.AppendLine($"- [{checkString}] {strikeThroughString}{item.Name}{strikeThroughString}");
        }

        sb.AppendLine();
        return sb.ToString();
    }

    private void AddCommon(StringBuilder sb)
    {
        sb.AppendLine($"# {_boardHolder.Board.Name}");
        sb.AppendLine();
    }

    private static void AppendHorizontalRule(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("_________________________________");
        sb.AppendLine();
    }
    
    private static string RemoveMultipleBlankLines(string markdownDocument)
    {
        return Regex.Replace(markdownDocument, @"(\r\n){2,}", "\r\n\r\n");
    }
}