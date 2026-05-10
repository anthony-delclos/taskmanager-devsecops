using TaskManager.API.DTOs.Mappers;
using TaskManager.API.DTOs.Requests;
using TaskManager.API.DTOs.Responses;
using TaskManager.API.Entities;
using TaskManager.API.Repositories.Interfaces;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository) => _repository = repository;

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(UserMapper.ToResponse);
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user == null ? null : UserMapper.ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        // A01 – prevent account creation with duplicate email
        if (await _repository.GetByEmailAsync(request.Email) != null)
            throw new InvalidOperationException("Email already registered.");

        string hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        CmUser user = UserMapper.ToEntity(request, hash);
        await _repository.AddAsync(user);
        await _repository.SaveAsync();
        return UserMapper.ToResponse(user);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        existing.Username = request.Username ?? existing.Username;
        existing.Email = request.Email ?? existing.Email;
        existing.IsAdmin = request.IsAdmin ?? existing.IsAdmin;
        existing.Status = request.Status != null
            ? Enum.Parse<Enums.Status>(request.Status)
            : existing.Status;

        if (!string.IsNullOrWhiteSpace(request.Password))
            existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        _repository.Update(existing);
        await _repository.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return false;

        _repository.Delete(existing);
        await _repository.SaveAsync();
        return true;
    }
}
