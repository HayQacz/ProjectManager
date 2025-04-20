using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ProjectManager.Services.Authorization;

public class ProjectAuthorizationService : IProjectAuthorizationService
{
    private readonly AppDbContext _db;
    public ProjectAuthorizationService(AppDbContext db) => _db = db;

    public async Task<bool> CanViewProject(Guid userId, Guid projectId)
    {
        var project = await _db.Projects
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project is null) return false;
        if (project.Details is null || project.Details.Visibility != ProjectVisibility.Private)
            return true;

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.ProjectId == projectId);

        return member is not null &&
               member.Role is ProjectMemberRole.Viewer
                   or ProjectMemberRole.Contributor
                   or ProjectMemberRole.Manager
                   or ProjectMemberRole.Owner;
    }

    public async Task<bool> CanEditProject(Guid userId, Guid projectId)
    {
        var member = await _db.ProjectMembers
                              .FirstOrDefaultAsync(pm =>
                                   pm.ProjectId == projectId &&
                                   pm.UserId    == userId);

        return member is not null &&
               (member.Role == ProjectMemberRole.Owner || member.Role == ProjectMemberRole.Manager);
    }

    public async Task<bool> CanDeleteProject(Guid userId, Guid projectId)
    {
        var member = await _db.ProjectMembers
                              .FirstOrDefaultAsync(pm =>
                                   pm.ProjectId == projectId &&
                                   pm.UserId    == userId);

        return member is not null && member.Role == ProjectMemberRole.Owner;
    }

    public async Task<bool> CanManageMembers(Guid userId, Guid projectId)
    {
        var member = await _db.ProjectMembers
                              .FirstOrDefaultAsync(pm =>
                                   pm.ProjectId == projectId &&
                                   pm.UserId    == userId);

        return member is not null &&
               member.Role is ProjectMemberRole.Owner or ProjectMemberRole.Manager;
    }
}
