using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using COPILmatic_back.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace COPILmatic_back.API.Entities;

[Table("cm_subject")]
public partial class CmSubject
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [StringLength(256)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("status")]
    [StringLength(100)]
    [Unicode(false)]
    public Status? Status { get; set; }

    [Column("description")]
    [Unicode(false)]
    public string? Description { get; set; }

    [Column("deadline")]
    public DateOnly? Deadline { get; set; }

    [Column("priority")]
    [StringLength(50)]
    [Unicode(false)]
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

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Column("assigned_user_id")]
    public Guid? AssignedUserId { get; set; }

    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [Column("group_id")]
    public Guid GroupId { get; set; }

    [ForeignKey("AssignedUserId")]
    [InverseProperty("CmSubjectAssignedUsers")]
    public virtual CmUser? AssignedUser { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("CmSubjects")]
    public virtual CmCategory? Category { get; set; }

    [InverseProperty("Subject")]
    public virtual ICollection<CmComment> CmComments { get; set; } = new List<CmComment>();

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("CmSubjectCreatedByUsers")]
    public virtual CmUser CreatedByUser { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CmSubjects")]
    public virtual CmCustomer? Customer { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("CmSubjects")]
    public virtual CmGroup Group { get; set; } = null!;
}
