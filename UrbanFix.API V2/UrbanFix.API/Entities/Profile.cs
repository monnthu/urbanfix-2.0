namespace UrbanFix.API.Entities;

public class Profile
{
    public Guid Id { get; set; }

    public string Role { get; set; } = "civilian";

    public string? FullName { get; set; }

    public string? NationalId { get; set; }

    public DateTime CreatedAt { get; set; }
}
