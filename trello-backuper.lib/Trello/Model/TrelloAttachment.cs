namespace trello_backuper.lib.Trello.Model;

public class TrelloAttachment
{
    public string Id { get; set; }

    public string Name { get; set; }

    public bool IsUpload { get; set; }

    public string FileName { get; set; }

    public long? Bytes { get; set; }

    public string MimeType { get; set; }

    public Uri Url { get; set; }
}