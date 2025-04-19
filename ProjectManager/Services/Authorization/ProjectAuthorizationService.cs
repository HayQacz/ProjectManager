using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ProjectManager.Services.Authorization;

public class ProjectAuthorizationService : IProjectAuthorizationService
{
    private readonly AppDbContext _db;

    public ProjectAuthorizationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> CanViewProject(Guid userId, Guid projectId)
    {
        var project = await _db.Projects
            .Include(p => p.Members)
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return false;

        if (project.Details is null || project.Details.Visibility != ProjectVisibility.Private)
            return true;

        return await _db.Users
            .AnyAsync(u => u.Id == userId &&
                           u.ProjectMemberId != null &&
                           project.Members.Any(pm => pm.Id == u.ProjectMemberId));
    }

    public async Task<bool> CanEditProject(Guid userId, Guid projectId)
    {
        var user = await _db.Users
            .Include(u => u.ProjectMember)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.ProjectMember == null)
            return false;

        var project = await _db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return false;

        return await _db.ProjectMembers
            .Where(pm => pm.Id == user.ProjectMemberId && project.Members.Contains(pm))
            .AnyAsync(pm => pm.Role == ProjectMemberRole.Owner || pm.Role == ProjectMemberRole.Manager);
    }

    public async Task<bool> CanDeleteProject(Guid userId, Guid projectId)
    {
        var user = await _db.Users
            .Include(u => u.ProjectMember)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.ProjectMember == null)
            return false;

        var project = await _db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return false;

        return await _db.ProjectMembers
            .Where(pm => pm.Id == user.ProjectMemberId && project.Members.Contains(pm))
            .AnyAsync(pm => pm.Role == ProjectMemberRole.Owner);
    }

    public async Task<bool> CanManageMembers(Guid userId, Guid projectId)
    {
        var user = await _db.Users
            .Include(u => u.ProjectMember)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.ProjectMember == null)
            return false;

        var project = await _db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return false;

        return await _db.ProjectMembers
            .Where(pm => pm.Id == user.ProjectMemberId && project.Members.Contains(pm))
            .AnyAsync(pm => pm.Role == ProjectMemberRole.Owner || pm.Role == ProjectMemberRole.Manager);
    }
}
