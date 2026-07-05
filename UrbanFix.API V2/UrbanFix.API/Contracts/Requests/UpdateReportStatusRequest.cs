using UrbanFix.API.Entities;

namespace UrbanFix.API.Contracts.Requests;

public class UpdateReportStatusRequest
{
    public ReportStatus? Status { get; set; }
}
