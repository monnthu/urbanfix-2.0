using UrbanFix.API.Entities;

namespace UrbanFix.API.Contracts.Responses;

public class ReportDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ReportCategory Category { get; set; }

    public ReportPriority? Priority { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public Guid CivilianUserId { get; set; }

    public Guid? InstitutionId { get; set; }

    public ReportStatus Status { get; set; }

    public string? AiCategory { get; set; }

    public string? AiPriority { get; set; }

    public decimal? AiConfidence { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<ReportImageDto> Images { get; set; } = [];
}
