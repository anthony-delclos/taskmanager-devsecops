namespace TaskManager.API.DTOs.Requests
{
    public class UpdateSubjectRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateOnly? Deadline { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public decimal? EstimatedLoadHours { get; set; }
        public decimal? ActualLoadHours { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? AssignedUserId { get; set; }
    }
}
