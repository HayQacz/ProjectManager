using Microsoft.EntityFrameworkCore;
using MediatR;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record AddProjectMemberToProjectCommand(
    Guid ProjectId,
    Guid UserId,
    Guid RequestingUserId) : IRequest<bool>;

public class AddProjectMemberToProjectHandler
        : IRequestHandler<AddProjectMemberToProjectCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IProjectAuthorizationService _auth;

    public AddProjectMemberToProjectHandler(
        AppDbContext db,
        IProjectAuthorizationService auth)
    {
        _db   = db;
        _auth = auth;
    }

    public async Task<bool> Handle(
        AddProjectMemberToProjectCommand request,
        CancellationToken ct)
    {
        if (!await _auth.CanManageMembers(request.RequestingUserId, request.ProjectId))
            throw new UnauthorizedAccessException("You do not have permission to add members.");

        var project = await _db.Projects
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
                     ?? throw new KeyNotFoundException("Project not found.");

        var user = await _db.Users
                            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        if (project.Members.Any(m => m.UserId == user.Id))
            throw new InvalidOperationException("The user is already a member.");

        var member = new ProjectMember
        {
            ProjectId    = project.Id,
            UserId       = user.Id,
            Role         = ProjectMemberRole.Contributor,
            DisplayName  = user.FullName
        };

        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
