using System.ComponentModel.DataAnnotations;

namespace Urbanfix.Models;

public class ReportFormViewModel
{
    [Required(ErrorMessage = "El titulo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El titulo no puede superar 200 caracteres.")]
    [Display(Name = "Titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripcion es obligatoria.")]
    [StringLength(2000, ErrorMessage = "La descripcion no puede superar 2000 caracteres.")]
    [Display(Name = "Descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una categoria.")]
    [Display(Name = "Categoria")]
    public ReportCategory Categoria { get; set; }

    [Display(Name = "Prioridad")]
    public ReportPriority? Prioridad { get; set; }

    [Required(ErrorMessage = "La latitud es obligatoria.")]
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    [Display(Name = "Latitud")]
    public decimal Latitud { get; set; }

    [Required(ErrorMessage = "La longitud es obligatoria.")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    [Display(Name = "Longitud")]
    public decimal Longitud { get; set; }

    [Display(Name = "Imagen")]
    public IFormFile? Imagen { get; set; }
}
