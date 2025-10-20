namespace auth_service.Data.Models;

public class Team
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<WhitelistedEmail> Emails { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}