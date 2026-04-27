using TaskManager.API.DTOs.Mappers;
using TaskManager.API.DTOs.Requests;
using TaskManager.API.DTOs.Responses;
using TaskManager.API.Entities;
using TaskManager.API.Repositories.Interfaces;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _repository;

        public SubjectService(ISubjectRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SubjectResponse>> GetAllAsync()
        {
            IEnumerable<CmSubject?> subjects = await _repository.GetAllAsync();
            return subjects.Select(subject => SubjectMapper.ToResponse(subject));
        }

        public async Task<SubjectResponse?> GetByIdAsync(Guid id)
        {
            CmSubject? subject = await _repository.GetByIdAsync(id);
            if (subject == null) return null;
            return SubjectMapper.ToResponse(subject);
        }

        public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest request)
        {
            CmSubject? subject = SubjectMapper.ToEntity(request);

            await _repository.AddAsync(subject);
            await _repository.SaveAsync();
            return SubjectMapper.ToResponse(subject);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateSubjectRequest request)
        {
            CmSubject? existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Name = request.Name ?? existing.Name;
            existing.Description = request.Description ?? existing.Description;
            existing.Deadline = request.Deadline ?? existing.Deadline;
            existing.Priority = request.Priority != null ? Enum.Parse<Enums.SubjectPriority>(request.Priority) : existing.Priority;
            existing.Status = request.Status != null ? Enum.Parse<Enums.Status>(request.Status) : existing.Status;
            existing.EstimatedLoadHours = request.EstimatedLoadHours ?? existing.EstimatedLoadHours;
            existing.ActualLoadHours = request.ActualLoadHours ?? existing.ActualLoadHours;
            existing.CategoryId = request.CategoryId ?? existing.CategoryId;
            existing.AssignedUserId = request.AssignedUserId ?? existing.AssignedUserId;

            _repository.Update(existing);
            await _repository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            CmSubject? existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            _repository.Delete(existing);
            await _repository.SaveAsync();
            return true;
        }

        
    }
}
