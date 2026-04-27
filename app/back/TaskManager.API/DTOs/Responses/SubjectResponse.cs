namespace TaskManager.API.DTOs.Responses
{
    public class SubjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateOnly? Deadline { get; set; }
        public decimal? EstimatedLoadHours { get; set; }
        public decimal? ActualLoadHours { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? EditionDate { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid? AssignedUserId { get; set; }
        public string? AssignedUserUsername { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUsername { get; set; } = null!;
    }
}
