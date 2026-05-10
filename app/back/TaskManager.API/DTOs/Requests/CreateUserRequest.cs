using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs.Requests;

public class CreateUserRequest
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = null!;

    public bool IsAdmin { get; set; } = false;
}
