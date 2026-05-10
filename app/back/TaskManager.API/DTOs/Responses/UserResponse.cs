namespace TaskManager.API.DTOs.Responses;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public DateTime CreationDate { get; set; }
    public string? Status { get; set; }
}
