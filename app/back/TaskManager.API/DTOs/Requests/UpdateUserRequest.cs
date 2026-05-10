using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs.Requests;

public class UpdateUserRequest
{
    [StringLength(100)]
    public string? Username { get; set; }

    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    public bool? IsAdmin { get; set; }

    public string? Status { get; set; }
}
