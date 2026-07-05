namespace Urbanfix.Models;

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
    public ReportStatus Status { get; set; } = ReportStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ReportImageDto> Images { get; set; } = [];
}
