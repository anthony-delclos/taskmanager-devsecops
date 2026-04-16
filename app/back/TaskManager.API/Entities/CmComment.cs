using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using COPILmatic_back.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace COPILmatic_back.API.Entities;

[Table("cm_comment")]
public partial class CmComment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("edition_date")]
    public DateTime? EditionDate { get; set; }

    [Column("content")]
    [Unicode(false)]
    public string Content { get; set; } = null!;

    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [Column("subject_id")]
    public Guid SubjectId { get; set; }

    [Column("status")]
    [StringLength(100)]
    [Unicode(false)]
    public Status? Status { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("CmComments")]
    public virtual CmUser? CreatedByUser { get; set; }

    [ForeignKey("SubjectId")]
    [InverseProperty("CmComments")]
    public virtual CmSubject Subject { get; set; } = null!;
}
