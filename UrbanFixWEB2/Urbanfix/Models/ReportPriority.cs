using System.ComponentModel.DataAnnotations;

namespace Urbanfix.Models;

public enum ReportPriority
{
    [Display(Name = "Baja")]
    Low,

    [Display(Name = "Media")]
    Medium,

    [Display(Name = "Alta")]
    High,

    [Display(Name = "Critica")]
    Critical
}
