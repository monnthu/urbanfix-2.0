using System.ComponentModel.DataAnnotations;

namespace Urbanfix.Models;

public enum ReportCategory
{
    [Display(Name = "Inundacion")]
    Flooding = 0,

    [Display(Name = "Bache")]
    Pothole = 1,

    [Display(Name = "Alumbrado publico")]
    Streetlight = 2,

    [Display(Name = "Basura")]
    Garbage = 3,

    [Display(Name = "Graffiti")]
    Graffiti = 4,

    [Display(Name = "Otro")]
    Other = 5
}
