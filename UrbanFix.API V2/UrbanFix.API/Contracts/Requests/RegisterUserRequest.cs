using System.ComponentModel.DataAnnotations;

namespace UrbanFix.API.Contracts.Requests;

public class RegisterUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string NationalId { get; set; } = string.Empty;
}
