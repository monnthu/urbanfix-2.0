namespace Urbanfix.Models;

public class ProfileDto
{
    public Guid Id { get; set; }

    public string Role { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string? NationalId { get; set; }

    public DateTime CreatedAt { get; set; }
}
