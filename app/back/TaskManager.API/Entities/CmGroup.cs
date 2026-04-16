using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using COPILmatic_back.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace COPILmatic_back.API.Entities;

[Table("cm_group")]
public partial class CmGroup
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

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("edition_date")]
    public DateTime? EditionDate { get; set; }

    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<CmSubject> CmSubjects { get; set; } = new List<CmSubject>();

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("CmGroups")]
    public virtual CmUser? CreatedByUser { get; set; }

    [InverseProperty("Group")]
    public virtual ICollection<GroupHasUser> GroupHasUsers { get; set; } = new List<GroupHasUser>();

    [ForeignKey("GroupId")]
    [InverseProperty("Groups")]
    public virtual ICollection<CmCategory> Categories { get; set; } = new List<CmCategory>();

    [ForeignKey("GroupId")]
    [InverseProperty("Groups")]
    public virtual ICollection<CmCustomer> Customers { get; set; } = new List<CmCustomer>();
}
