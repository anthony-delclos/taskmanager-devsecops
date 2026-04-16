using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using COPILmatic_back.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace COPILmatic_back.API.Entities;

[Table("cm_user")]
public partial class CmUser
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    [StringLength(100)]
    [Unicode(false)]
    public string Username { get; set; } = null!;

    [Column("email")]
    [StringLength(256)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(256)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    [Column("is_admin")]
    public bool? IsAdmin { get; set; }


    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("status")]
    [StringLength(100)]
    [Unicode(false)]
    public Status? Status { get; set; }

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<CmComment> CmComments { get; set; } = new List<CmComment>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<CmGroup> CmGroups { get; set; } = new List<CmGroup>();

    [InverseProperty("AssignedUser")]
    public virtual ICollection<CmSubject> CmSubjectAssignedUsers { get; set; } = new List<CmSubject>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<CmSubject> CmSubjectCreatedByUsers { get; set; } = new List<CmSubject>();

    [InverseProperty("User")]
    public virtual ICollection<GroupHasUser> GroupHasUsers { get; set; } = new List<GroupHasUser>();

    [InverseProperty("User")]
    public ICollection<CmRefreshToken> CmRefreshTokens { get; set; } = new List<CmRefreshToken>();
}
