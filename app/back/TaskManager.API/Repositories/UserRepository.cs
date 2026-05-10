using Microsoft.EntityFrameworkCore;
using TaskManager.API.Entities;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CmUser>> GetAllAsync() =>
        await _db.CmUsers
            .Where(u => u.Status != Enums.Status.DELETED)
            .OrderByDescending(u => u.CreationDate)
            .ToListAsync();

    public async Task<CmUser?> GetByIdAsync(Guid id) =>
        await _db.CmUsers.FindAsync(id);

    public async Task<CmUser?> GetByEmailAsync(string email) =>
        await _db.CmUsers.FirstOrDefaultAsync(u => u.Email == email);

    public async Task AddAsync(CmUser user)
    {
        user.Id = Guid.NewGuid();
        user.CreationDate = DateTime.UtcNow;
        await _db.CmUsers.AddAsync(user);
    }

    public void Update(CmUser user)
    {
        var existing = _db.CmUsers.Find(user.Id);
        if (existing == null) return;

        existing.Username = user.Username;
        existing.Email = user.Email;
        existing.PasswordHash = user.PasswordHash;
        existing.IsAdmin = user.IsAdmin;
        existing.Status = user.Status;
    }

    public void Delete(CmUser user)
    {
        user.Status = Enums.Status.DELETED;
    }

    public async Task SaveAsync() => await _db.SaveChangesAsync();
}
