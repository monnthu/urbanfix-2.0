namespace Urbanfix.Models;

public class ReportImageDto
{
    public Guid Id { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public short SortOrder { get; set; }
}
