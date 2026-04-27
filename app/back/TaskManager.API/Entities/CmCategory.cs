using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.API.Entities;

[Table("cm_category")]
public class CmCategory
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public virtual ICollection<CmSubject> Subjects { get; set; } = new List<CmSubject>();
}