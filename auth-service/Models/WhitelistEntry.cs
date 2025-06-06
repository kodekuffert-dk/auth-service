namespace auth_service.Models;

public class WhitelistEntry
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}