using System.ComponentModel.DataAnnotations;
using Urbanfix.Helpers;

namespace Urbanfix.Models;

public class RegisterViewModel : IValidatableObject
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electronico valido.")]
    [Display(Name = "Correo electronico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contrasena debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contrasena")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma tu contrasena.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contrasenas no coinciden.")]
    [Display(Name = "Confirmar contrasena")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar 200 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DUI o ID personal es obligatorio.")]
    [Display(Name = "DUI o ID personal")]
    public string NationalId { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!SalvadoranIdValidator.TryNormalize(NationalId, out _))
        {
            yield return new ValidationResult(
                "El DUI debe tener 8 digitos, guion y digito verificador (ej. 12345678-9).",
                [nameof(NationalId)]);
        }
    }
}
