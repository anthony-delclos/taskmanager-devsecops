using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace COPILmatic_back.API.Entities;

public partial class CopilmaticContext : DbContext
{
    public CopilmaticContext(DbContextOptions<CopilmaticContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CmCategory> CmCategories { get; set; }

    public virtual DbSet<CmComment> CmComments { get; set; }

    public virtual DbSet<CmCustomer> CmCustomers { get; set; }

    public virtual DbSet<CmGroup> CmGroups { get; set; }

    public virtual DbSet<CmSubject> CmSubjects { get; set; }

    public virtual DbSet<CmUser> CmUsers { get; set; }

    public virtual DbSet<GroupHasUser> GroupHasUsers { get; set; }

    public virtual DbSet<CmRefreshToken> CmRefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CmCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cm_categ__3213E83F8AD1C9F5");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<CmComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cm_comme__3213E83FB46C06D5");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CmComments).HasConstraintName("FK__cm_commen__creat__3C69FB99");

            entity.HasOne(d => d.Subject).WithMany(p => p.CmComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cm_commen__subje__3D5E1FD2");
        });

        modelBuilder.Entity<CmCustomer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cm_custo__3213E83FD627A2CB");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(c => c.AnnualAllocatedLoadDays).HasColumnType("decimal(6,2)");

            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<CmGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cm_group__3213E83FBA5438E2");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CmGroups).HasConstraintName("FK__cm_group__create__2A4B4B5E");

            entity.HasMany(d => d.Categories).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupHasCategory",
                    r => r.HasOne<CmCategory>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GROUP_HAS__categ__44FF419A"),
                    l => l.HasOne<CmGroup>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GROUP_HAS__group__440B1D61"),
                    j =>
                    {
                        j.HasKey("GroupId", "CategoryId").HasName("PK__GROUP_HA__88237B3BFE958F51");
                        j.ToTable("GROUP_HAS_CATEGORY");
                        j.IndexerProperty<Guid>("GroupId").HasColumnName("group_id");
                        j.IndexerProperty<Guid>("CategoryId").HasColumnName("category_id");
                    });

            entity.HasMany(d => d.Customers).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupHasCustomer",
                    r => r.HasOne<CmCustomer>().WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GROUP_HAS__custo__412EB0B6"),
                    l => l.HasOne<CmGroup>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__GROUP_HAS__group__403A8C7D"),
                    j =>
                    {
                        j.HasKey("GroupId", "CustomerId").HasName("PK__GROUP_HA__99A1C9188F4A2A6F");
                        j.ToTable("GROUP_HAS_CUSTOMER");
                        j.IndexerProperty<Guid>("GroupId").HasColumnName("group_id");
                        j.IndexerProperty<Guid>("CustomerId").HasColumnName("customer_id");
                    });
        });

        modelBuilder.Entity<CmSubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cm_subje__3213E83F35FFC490");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.Status).HasConversion<string>();

            entity.Property(e => e.Priority).HasConversion<string>();

            entity.Property(s => s.EstimatedLoadHours).HasColumnType("decimal(6,2)");

            entity.Property(s => s.ActualLoadHours).HasColumnType("decimal(6,2)");

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.CmSubjectAssignedUsers).HasConstraintName("FK__cm_subjec__assig__37A5467C");

            entity.HasOne(d => d.Category).WithMany(p => p.CmSubjects).HasConstraintName("FK__cm_subjec__categ__35BCFE0A");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.CmSubjectCreatedByUsers).HasConstraintName("FK__cm_subjec__creat__38996AB5");

            entity.HasOne(d => d.Customer).WithMany(p => p.CmSubjects).HasConstraintName("FK__cm_subjec__custo__36B12243");

            entity.HasOne(d => d.Group).WithMany(p => p.CmSubjects)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cm_subjec__group__398D8EEE");
        });

        modelBuilder.Entity<CmUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cm_user__3213E83F52BF5AD0");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasMany(u => u.CmRefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .HasConstraintName("FK__cm_refresh_token__user");
        });

        modelBuilder.Entity<GroupHasUser>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.GroupId }).HasName("PK__GROUP_HA__A4E94E55281D401F");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupHasUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GROUP_HAS__group__48CFD27E");

            entity.HasOne(d => d.User).WithMany(p => p.GroupHasUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GROUP_HAS__user___47DBAE45");
        });

        modelBuilder.Entity<CmRefreshToken>(entity =>
        {
            entity.ToTable("cm_refresh_token");

            entity.HasKey(e => e.Id)
                  .HasName("PK__cm_refresh_token");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.TokenHash)
                  .HasColumnName("token_hash")
                  .HasColumnType("char(64)")
                  .IsFixedLength()
                  .IsRequired();

            entity.HasIndex(e => e.TokenHash)
                  .IsUnique()
                  .HasDatabaseName("UQ_cm_refresh_token_hash");

            entity.Property(e => e.ExpiresAt)
                  .HasColumnName("expires_at");

            entity.Property(e => e.Revoked)
                  .HasColumnName("revoked");

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.HasOne(d => d.User)
                  .WithMany(p => p.CmRefreshTokens)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__cm_refresh_token__user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
