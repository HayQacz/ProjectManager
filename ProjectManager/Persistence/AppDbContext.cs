using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;

namespace ProjectManager.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectDetails> ProjectDetails => Set<ProjectDetails>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<ProjectCategory> ProjectCategories => Set<ProjectCategory>();
    public DbSet<ProjectCategoryItem> ProjectCategoryItems => Set<ProjectCategoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Details)
            .WithOne(d => d.Project)
            .HasForeignKey<ProjectDetails>(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
