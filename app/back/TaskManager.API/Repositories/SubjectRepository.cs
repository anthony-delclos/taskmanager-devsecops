using Microsoft.EntityFrameworkCore;
using System;
using TaskManager.API.Entities;
using TaskManager.API.Repositories.Interfaces;

namespace TaskManager.API.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly AppDbContext _db;

        public SubjectRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CmSubject>> GetAllAsync()
        {
            return await _db.CmSubjects
                .Include(s => s.Category)
                .Include(s => s.AssignedUser)
                .OrderByDescending(s => s.CreationDate)
                .ToListAsync();
        }

        public async Task<CmSubject?> GetByIdAsync(Guid id)
        {
            return await _db.CmSubjects
                .Include(s => s.Category)
                .Include(s => s.AssignedUser)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(CmSubject subject)
        {
            subject.Id = Guid.NewGuid();
            subject.CreationDate = DateTime.UtcNow;
            await _db.CmSubjects.AddAsync(subject);
        }

        public void Update(CmSubject subject)
        {
            var existing = _db.CmSubjects.Find(subject.Id);
            if (existing == null) return;

            existing.Name = subject.Name;
            existing.Description = subject.Description;
            existing.Status = subject.Status;
            existing.Priority = subject.Priority;
            existing.Deadline = subject.Deadline;
            existing.EstimatedLoadHours = subject.EstimatedLoadHours;
            existing.ActualLoadHours = subject.ActualLoadHours;
            existing.CategoryId = subject.CategoryId;
            existing.AssignedUserId = subject.AssignedUserId;
            existing.EditionDate = DateTime.UtcNow;
        }

        public void Delete(CmSubject subject)
        {
            subject.Status = Enums.Status.DELETED;
            subject.EditionDate = DateTime.UtcNow;
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
