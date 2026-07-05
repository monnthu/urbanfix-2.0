using System.ComponentModel.DataAnnotations;

namespace Urbanfix.Models;

public enum ReportStatus
{
    [Display(Name = "Abierto")]
    Open = 0,

    [Display(Name = "Asignado")]
    Assigned = 1,

    [Display(Name = "En progreso")]
    InProgress = 2,

    [Display(Name = "Resuelto")]
    Resolved = 3,

    [Display(Name = "Sin asignar")]
    Unassigned = 4
}
