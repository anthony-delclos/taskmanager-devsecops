using TaskManager.API.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.API.Entities;

[Table("cm_subject")]
public class CmSubject
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    [Column("status")]
    [StringLength(100)]
    public Status? Status { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("deadline")]
    public DateOnly? Deadline { get; set; }

    [Column("priority")]
    [StringLength(50)]
    public SubjectPriority? Priority { get; set; }

    [Column("estimated_load_hours")]
    public decimal? EstimatedLoadHours { get; set; }

    [Column("actual_load_hours")]
    public decimal? ActualLoadHours { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("edition_date")]
    public DateTime? EditionDate { get; set; }

    [Column("category_id")]
    public Guid? CategoryId { get; set; }

    [Column("assigned_user_id")]
    public Guid? AssignedUserId { get; set; }

    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [ForeignKey("CategoryId")]
    public virtual CmCategory? Category { get; set; }

    [ForeignKey("AssignedUserId")]
    public virtual CmUser? AssignedUser { get; set; }

    [ForeignKey("CreatedByUserId")]
    public virtual CmUser CreatedByUser { get; set; } = null!;
}