using TaskManager.API.Entities;

namespace TaskManager.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<CmUser>> GetAllAsync();
    Task<CmUser?> GetByIdAsync(Guid id);
    Task<CmUser?> GetByEmailAsync(string email);
    Task AddAsync(CmUser user);
    void Update(CmUser user);
    void Delete(CmUser user);
    Task SaveAsync();
}
