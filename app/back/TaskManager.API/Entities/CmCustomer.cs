using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using COPILmatic_back.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace COPILmatic_back.API.Entities;

[Table("cm_customer")]
public partial class CmCustomer
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_name")]
    [StringLength(100)]
    [Unicode(false)]
    public string CompanyName { get; set; } = null!;

    [Column("annual_allocated_load_days")]
    public decimal? AnnualAllocatedLoadDays { get; set; }

    [Column("status")]
    [StringLength(100)]
    [Unicode(false)]
    public Status? Status { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<CmSubject> CmSubjects { get; set; } = new List<CmSubject>();

    [ForeignKey("CustomerId")]
    [InverseProperty("Customers")]
    public virtual ICollection<CmGroup> Groups { get; set; } = new List<CmGroup>();
}
