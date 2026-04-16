using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<CmUser> CmUsers { get; set; }
    public DbSet<CmSubject> CmSubjects { get; set; }
    public DbSet<CmCategory> CmCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CmUser>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("UUID()");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasMany(u => u.AssignedSubjects)
                .WithOne(s => s.AssignedUser)
                .HasForeignKey(s => s.AssignedUserId);
            entity.HasMany(u => u.CreatedSubjects)
                .WithOne(s => s.CreatedByUser)
                .HasForeignKey(s => s.CreatedByUserId);
        });

        modelBuilder.Entity<CmCategory>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("UUID()");
            entity.HasMany(c => c.Subjects)
                .WithOne(s => s.Category)
                .HasForeignKey(s => s.CategoryId);
        });

        modelBuilder.Entity<CmSubject>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("UUID()");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Priority).HasConversion<string>();
            entity.Property(s => s.EstimatedLoadHours).HasColumnType("decimal(6,2)");
            entity.Property(s => s.ActualLoadHours).HasColumnType("decimal(6,2)");
        });

        // Seed des catégories
        modelBuilder.Entity<CmCategory>().HasData(
            new CmCategory { Id = Guid.Parse("1f170089-7223-4f5a-9902-b0354fbe4e7a"), Name = "Support" },
            new CmCategory { Id = Guid.Parse("236b71aa-9ef4-4e06-b951-6598062aa199"), Name = "Debug" },
            new CmCategory { Id = Guid.Parse("57f93148-b137-4324-91cd-702dcdb7d562"), Name = "Optimisation" },
            new CmCategory { Id = Guid.Parse("150ab93d-d6a1-47c2-bff4-9fa6f0998a06"), Name = "Etude" },
            new CmCategory { Id = Guid.Parse("fdc5572c-bec1-4b7e-8efc-03fd3e1a2776"), Name = "Evolution" }
        );
    }
}