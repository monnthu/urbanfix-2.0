using UrbanFix.API.Entities;

namespace UrbanFix.API.Contracts.Requests;

public class CreateReportRequest
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public ReportCategory Category { get; set; }

    public ReportPriority? Priority { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public List<IFormFile>? Images { get; set; }
}
