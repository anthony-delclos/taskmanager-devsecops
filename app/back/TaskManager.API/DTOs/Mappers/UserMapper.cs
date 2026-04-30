using TaskManager.API.DTOs.Requests;
using TaskManager.API.DTOs.Responses;
using TaskManager.API.Entities;

namespace TaskManager.API.DTOs.Mappers;

public static class UserMapper
{
    public static UserResponse ToResponse(CmUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        IsAdmin = user.IsAdmin,
        CreationDate = user.CreationDate,
        Status = user.Status?.ToString()
    };

    public static CmUser ToEntity(CreateUserRequest request, string passwordHash) => new()
    {
        Username = request.Username,
        Email = request.Email,
        PasswordHash = passwordHash,
        IsAdmin = request.IsAdmin
    };
}
