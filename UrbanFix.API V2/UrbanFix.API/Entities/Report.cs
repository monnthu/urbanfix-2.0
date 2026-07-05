namespace UrbanFix.API.Entities;

public class Report
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public ReportCategory Category { get; set; }

    public ReportPriority? Priority { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public Guid CivilianUserId { get; set; }

    public Guid? InstitutionId { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public string? AiCategory { get; set; }

    public string? AiPriority { get; set; }

    public decimal? AiConfidence { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ReportImage> Images { get; set; } = [];
}
