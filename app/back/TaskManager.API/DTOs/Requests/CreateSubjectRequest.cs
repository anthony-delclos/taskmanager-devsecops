namespace TaskManager.API.DTOs.Requests
{
    public class CreateSubjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly? Deadline { get; set; }
        public string? Priority { get; set; }
        public decimal? EstimatedLoadHours { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
