using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Urbanfix.Models;

public enum InstitutionStatus
{
    [Display(Name = "Pendiente")]
    Pending = 0,

    [Display(Name = "Verificada")]
    Verified = 1,

    [Display(Name = "Rechazada")]
    Rejected = 2
}

public class ServiceZone
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public decimal MinLatitude { get; set; }

    public decimal MaxLatitude { get; set; }

    public decimal MinLongitude { get; set; }

    public decimal MaxLongitude { get; set; }
}

public class InstitutionApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string ProfileId { get; set; }

    public required string ContactEmail { get; set; }

    public required string InstitutionName { get; set; }

    public required string OfficialEmail { get; set; }

    public required string OfficialDomain { get; set; }

    public string? Department { get; set; }

    public required string ServiceZoneCode { get; set; }

    public List<ReportCategory> CoverageCategories { get; set; } = [];

    public string? ProofDocumentFileName { get; set; }

    public InstitutionStatus Status { get; set; } = InstitutionStatus.Pending;

    public string? ReviewNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }
}

public class InstitutionRegistrationViewModel
{
    [Required(ErrorMessage = "Ingresa el nombre de la institucion.")]
    [Display(Name = "Nombre de la institucion")]
    public string InstitutionName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa un correo institucional.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
    [Display(Name = "Correo oficial")]
    public string OfficialEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa el dominio oficial.")]
    [Display(Name = "Dominio oficial")]
    public string OfficialDomain { get; set; } = string.Empty;

    [Display(Name = "Departamento o unidad")]
    public string? Department { get; set; }

    [Required(ErrorMessage = "Selecciona una zona de servicio.")]
    [Display(Name = "Zona de servicio")]
    public string ServiceZoneCode { get; set; } = string.Empty;

    [Display(Name = "Categorias cubiertas")]
    public List<ReportCategory> CoverageCategories { get; set; } = [];

    [Display(Name = "Documento de respaldo")]
    public IFormFile? ProofDocument { get; set; }

    public IReadOnlyList<SelectListItem> ZoneOptions { get; set; } = [];
}

public class RoutedReportViewModel
{
    public Guid ReportId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CategoryLabel { get; set; } = string.Empty;

    public string PriorityLabel { get; set; } = string.Empty;

    public string StatusLabel { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class InstitutionDashboardViewModel
{
    public required InstitutionApplication Institution { get; set; }

    public required string ServiceZoneName { get; set; }

    public IReadOnlyList<RoutedReportViewModel> AssignedReports { get; set; } = [];

    public string? Error { get; set; }
}

public class AdminInstitutionApprovalViewModel
{
    public Guid Id { get; set; }

    public string InstitutionName { get; set; } = string.Empty;

    public string OfficialEmail { get; set; } = string.Empty;

    public string OfficialDomain { get; set; } = string.Empty;

    public string ServiceZoneName { get; set; } = string.Empty;

    public string CoverageSummary { get; set; } = string.Empty;

    public string StatusLabel { get; set; } = string.Empty;

    public InstitutionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class AdminDashboardViewModel
{
    public IReadOnlyList<AdminReportStatusViewModel> Reports { get; set; } = [];

    public IReadOnlyList<AdminInstitutionApprovalViewModel> Institutions { get; set; } = [];
}
