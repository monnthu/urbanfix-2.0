namespace UrbanFix.API.Entities;

public class Institution
{
    public Guid Id { get; set; }

    public Guid ProfileId { get; set; }

    public required string Name { get; set; }

    public required string OfficialDomain { get; set; }

    public string? Category { get; set; }

    public string? Zone { get; set; }

    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
