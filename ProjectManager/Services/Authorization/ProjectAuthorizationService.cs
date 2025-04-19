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

        if (project is null)
            return false;

        if (project.Details is null || project.Details.Visibility != ProjectVisibility.Private)
            return true;

        var user = await _db.Users
            .Include(u => u.ProjectMember)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.ProjectMember is null)
            return false;

        var isMember = project.Members.Any(pm => pm.Id == user.ProjectMemberId);
        var hasAccess = user.ProjectMember.Role is ProjectMemberRole.Contributor
            or ProjectMemberRole.Manager
            or ProjectMemberRole.Owner;

        return isMember && hasAccess;
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

        if (project is null)
            return false;

        return project.Members.Any(pm =>
            pm.Id == user.ProjectMemberId &&
            (pm.Role == ProjectMemberRole.Owner || pm.Role == ProjectMemberRole.Manager));
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

        if (project is null)
            return false;

        return project.Members.Any(pm =>
            pm.Id == user.ProjectMemberId &&
            pm.Role == ProjectMemberRole.Owner);
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

        if (project is null)
            return false;

        return project.Members.Any(pm =>
            pm.Id == user.ProjectMemberId &&
            (pm.Role == ProjectMemberRole.Owner || pm.Role == ProjectMemberRole.Manager));
    }
}
