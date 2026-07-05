namespace Urbanfix.Models;

public class AdminReportStatusViewModel
{
    public Guid ReportId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ReportStatus Status { get; set; }

    public string StatusLabel { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
