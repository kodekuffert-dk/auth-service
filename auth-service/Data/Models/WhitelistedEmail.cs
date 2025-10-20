namespace auth_service.Data.Models;

public class WhitelistedEmail
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
}
