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
    public DbSet<User> Users => Set<User>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Details)
            .WithOne(d => d.Project)
            .HasForeignKey<ProjectDetails>(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Members)
            .WithMany(m => m.Projects)
            .UsingEntity(j => j.ToTable("ProjectMembersProjects"));

        modelBuilder.Entity<User>()
            .HasOne(u => u.ProjectMember)
            .WithOne(pm => pm.User)
            .HasForeignKey<User>(u => u.ProjectMemberId);
        
        modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.AssignedMember)
            .WithMany()
            .HasForeignKey(t => t.AssignedMemberId)
            .OnDelete(DeleteBehavior.SetNull);

    }

}
