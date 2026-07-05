using Urbanfix.Models;

namespace Urbanfix.Services;

public class InstitutionRoutingService
{
    private readonly InstitutionDirectoryService _institutionDirectoryService;

    public InstitutionRoutingService(InstitutionDirectoryService institutionDirectoryService)
    {
        _institutionDirectoryService = institutionDirectoryService;
    }

    public IReadOnlyList<RoutedReportViewModel> GetAssignedReports(
        InstitutionApplication institution,
        IEnumerable<ReportDto> reports)
    {
        return reports
            .Where(report => IsAssignedToInstitution(institution, report))
            .OrderByDescending(report => report.CreatedAt)
            .Select(report => new RoutedReportViewModel
            {
                ReportId = report.Id,
                Title = report.Title,
                CategoryLabel = report.CategoryLabel,
                PriorityLabel = report.PriorityLabel,
                StatusLabel = report.StatusLabel,
                CreatedAt = report.CreatedAt
            })
            .ToList();
    }

    private bool IsAssignedToInstitution(InstitutionApplication institution, ReportDto report)
    {
        if (!institution.CoverageCategories.Contains(report.Category))
        {
            return false;
        }

        var reportZoneCode = _institutionDirectoryService.ResolveZoneCode(
            report.Latitude,
            report.Longitude);

        return string.Equals(
            institution.ServiceZoneCode,
            reportZoneCode,
            StringComparison.OrdinalIgnoreCase);
    }
}
