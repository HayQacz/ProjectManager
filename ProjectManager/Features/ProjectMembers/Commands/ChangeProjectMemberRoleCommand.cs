using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Entities.Enums;
using ProjectManager.Features.ProjectMembers.Models;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record ChangeProjectMemberRoleCommand(Guid MemberId, ProjectMemberRole NewRole, Guid RequestingUserId) : IRequest<ProjectMemberDto?>;

public class ChangeProjectMemberRoleHandler : IRequestHandler<ChangeProjectMemberRoleCommand, ProjectMemberDto?>
{
    private readonly AppDbContext _db;
    private readonly IProjectAuthorizationService _auth;

    public ChangeProjectMemberRoleHandler(AppDbContext db, IProjectAuthorizationService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task<ProjectMemberDto?> Handle(ChangeProjectMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var member = await _db.ProjectMembers
            .Include(pm => pm.User)
            .Include(pm => pm.Projects)
            .FirstOrDefaultAsync(pm => pm.Id == request.MemberId, cancellationToken);

        if (member == null || !member.Projects.Any())
            return null;

        var projectId = member.Projects.First().Id;

        if (!await _auth.CanManageMembers(request.RequestingUserId, projectId))
            return null;

        member.Role = request.NewRole;
        await _db.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto
        {
            Id = member.Id,
            Role = member.Role,
            Email = member.User?.Email,
            DisplayName = member.DisplayName
        };
    }
}
