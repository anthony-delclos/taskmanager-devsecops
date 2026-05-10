using TaskManager.API.Entities;

namespace TaskManager.API.Repositories.Interfaces
{
    public interface ISubjectRepository
    {
        Task<IEnumerable<CmSubject>> GetAllAsync();
        Task<CmSubject?> GetByIdAsync(Guid id);
        Task AddAsync(CmSubject subject);
        void Update(CmSubject subject);
        void Delete(CmSubject subject);
        Task SaveAsync();
    }
}
