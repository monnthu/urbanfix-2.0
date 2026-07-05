namespace UrbanFix.API.Entities;

public class ReportImage
{
    public Guid Id { get; set; }

    public Guid ReportId { get; set; }

    public Report Report { get; set; } = null!;

    /// <summary>
    /// Ruta del objeto en Supabase Storage (p. ej. reports/{reportId}/{imageId}.jpg).
    /// </summary>
    public required string StoragePath { get; set; }

    public string? ThumbnailPath { get; set; }

    public required string ContentType { get; set; }

    public long FileSizeBytes { get; set; }

    public short SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
