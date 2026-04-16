using TaskManager.API.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.API.Entities;

[Table("cm_user")]
public class CmUser
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    [StringLength(100)]
    public string Username { get; set; } = null!;

    [Column("email")]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(256)]
    public string PasswordHash { get; set; } = null!;

    [Column("is_admin")]
    public bool IsAdmin { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("status")]
    [StringLength(100)]
    public Status? Status { get; set; }

    public virtual ICollection<CmSubject> AssignedSubjects { get; set; } = new List<CmSubject>();
    public virtual ICollection<CmSubject> CreatedSubjects { get; set; } = new List<CmSubject>();
}