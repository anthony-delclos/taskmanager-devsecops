using TaskManager.API.DTOs.Requests;
using TaskManager.API.DTOs.Responses;
using TaskManager.API.Entities;

namespace TaskManager.API.DTOs.Mappers
{
    public static class SubjectMapper
    {
        public static SubjectResponse ToResponse(CmSubject s) => new()
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Status = s.Status?.ToString(),
            Priority = s.Priority?.ToString(),
            Deadline = s.Deadline,
            EstimatedLoadHours = s.EstimatedLoadHours,
            ActualLoadHours = s.ActualLoadHours,
            CreationDate = s.CreationDate,
            EditionDate = s.EditionDate,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.Name,
            AssignedUserId = s.AssignedUserId,
            AssignedUserUsername = s.AssignedUser?.Username,
            CreatedByUserId = s.CreatedByUserId,
            CreatedByUsername = s.CreatedByUser?.Username ?? string.Empty
        };

        public static CmSubject ToEntity(CreateSubjectRequest request) => new()
        {
            Name = request.Name,
            Description = request.Description,
            Deadline = request.Deadline,
            Priority = request.Priority != null ? Enum.Parse<Enums.SubjectPriority>(request.Priority) : null,
            EstimatedLoadHours = request.EstimatedLoadHours,
            CategoryId = request.CategoryId,
            AssignedUserId = request.AssignedUserId,
            CreatedByUserId = request.CreatedByUserId
        };
    }
}
