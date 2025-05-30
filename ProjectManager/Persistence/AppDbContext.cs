﻿// AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;

namespace ProjectManager.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project>          Projects          => Set<Project>();
    public DbSet<ProjectDetails>   ProjectDetails    => Set<ProjectDetails>();
    public DbSet<ProjectMember>    ProjectMembers    => Set<ProjectMember>();
    public DbSet<ProjectTask>      ProjectTasks      => Set<ProjectTask>();
    public DbSet<User>             Users             => Set<User>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Project>()
            .HasOne(p => p.Details)
            .WithOne(d => d.Project)
            .HasForeignKey<ProjectDetails>(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p  => p.Members)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany(u  => u.ProjectMembers)
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ProjectTask>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ProjectTask>()
            .HasOne(t => t.AssignedMember)
            .WithMany()
            .HasForeignKey(t => t.AssignedMemberId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
