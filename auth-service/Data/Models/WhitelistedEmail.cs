namespace auth_service.Data.Models;

public class WhitelistedEmail
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
