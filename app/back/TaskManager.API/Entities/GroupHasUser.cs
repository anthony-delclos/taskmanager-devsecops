using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COPILmatic_back.API.Entities;

[PrimaryKey("UserId", "GroupId")]
[Table("GROUP_HAS_USER")]
public partial class GroupHasUser
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Key]
    [Column("group_id")]
    public Guid GroupId { get; set; }

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    [ForeignKey("GroupId")]
    [InverseProperty("GroupHasUsers")]
    public virtual CmGroup Group { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("GroupHasUsers")]
    public virtual CmUser User { get; set; } = null!;
}
