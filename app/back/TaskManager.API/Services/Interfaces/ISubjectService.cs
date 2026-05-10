using TaskManager.API.DTOs.Requests;
using TaskManager.API.DTOs.Responses;
using TaskManager.API.Entities;

namespace TaskManager.API.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<IEnumerable<SubjectResponse>> GetAllAsync();
        Task<SubjectResponse?> GetByIdAsync(Guid id);
        Task<SubjectResponse> CreateAsync(CreateSubjectRequest request);
        Task<bool> UpdateAsync(Guid id, UpdateSubjectRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
